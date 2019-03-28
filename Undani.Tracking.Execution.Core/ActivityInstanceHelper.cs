using System.Linq;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Undani.Tracking.Execution.Core.Infra;
using Newtonsoft.Json;
using Undani.Tracking.Execution.Core.Resource;
using Microsoft.Extensions.Configuration;

namespace Undani.Tracking.Execution.Core
{
    public class ActivityInstanceHelper : Helper
    {
        public ActivityInstanceHelper(IConfiguration configuration, Guid userId, string token = "") : base(configuration, userId, token) { }

        public string IsAnonymous(Guid activityInstanceRefId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ActivityInstanceIsAnonymous", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceRefId", SqlDbType.UniqueIdentifier) { Value = activityInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@IsAnonymous", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();

                    if ((bool)cmd.Parameters["@IsAnonymous"].Value)
                        return Token;
                    else
                        return "";
                }
            }
        }

        public ActivityInstance Get(Guid activityInstanceRefId)
        {
            ActivityInstance activity;
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ActivityInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceRefId", SqlDbType.UniqueIdentifier) { Value = activityInstanceRefId });

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count == 1)
                        {
                            DataRow dr = dt.Rows[0];
                            activity = new ActivityInstance()
                            {
                                RefId = activityInstanceRefId,
                                Name = (string)dr["ActivityName"],
                                CoustomViewer = (string)dr["ActivityCoustomViewer"],
                                ActionButtonsDisabled = (bool)dr["ActionsDisabled"],
                                Start = (DateTime)dr["StartDate"],
                                FormInstanceId = SetFormInstance((Guid)dr["FormInstanceId"], (int)dr["Id"]),
                                FormReadOnly = (bool)dr["ActivityFormReadOnly"],
                                FlowInstanceSummary = new FlowInstanceHelper(Configuration, UserId, Token).GetSummary((int)dr["FlowInstanceId"])
                            };

                            if (dr["EndDate"] != DBNull.Value)
                                activity.End = (DateTime)dr["EndDate"];
                            else if ((Guid)dr["UserId"] == UserId)
                                activity.ActionButtons = new ActionInstanceHelper(Configuration, UserId, Token).GetActions((string)dr["ActivityId"]);
                        }
                        else
                            throw new Exception("Could not get the activity");
                    }
                }
            }

            return activity;
        }

        private Guid SetFormInstance(Guid formInstanceId, int activityInstanceId)
        {
            if (formInstanceId == Guid.Empty)
            {
                var _activityInstance = new _ActivityInstance();

                using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
                {
                    cn.Open();

                    using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ActivityInstanceFormInstance", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ActivityInstanceId", SqlDbType.Int) { Value = activityInstanceId });
                        cmd.Parameters.Add(new SqlParameter("@EnvironmentId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                        cmd.Parameters.Add(new SqlParameter("@FormId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                        cmd.Parameters.Add(new SqlParameter("@FormVersion", SqlDbType.Int) { Direction = ParameterDirection.Output });
                        cmd.Parameters.Add(new SqlParameter("@FormReadOnly", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                        cmd.Parameters.Add(new SqlParameter("@FormInstanceParentId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                        cmd.Parameters.Add(new SqlParameter("@ActivityTypeId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                        cmd.ExecuteNonQuery();

                        if ((int)cmd.Parameters["@ActivityTypeId"].Value == 1)
                        {
                            _activityInstance.EnvironmentId = (Guid)cmd.Parameters["@EnvironmentId"].Value;
                            _activityInstance.FormId = (Guid)cmd.Parameters["@FormId"].Value;
                            _activityInstance.FormVersion = (int)cmd.Parameters["@FormVersion"].Value;
                            _activityInstance.FormReadOnly = (bool)cmd.Parameters["@FormReadOnly"].Value;
                            _activityInstance.FormParentInstanceId = (Guid)cmd.Parameters["@FormInstanceParentId"].Value;

                            _activityInstance.FormInstanceId = new FormCall(Configuration).GetInstance(_activityInstance, Token);

                            cmd.CommandText = "EXECUTION.usp_Set_ActivityInstanceFormInstanceId";
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add(new SqlParameter("@ActivityInstanceId", SqlDbType.Int) { Value = activityInstanceId });
                            cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Value = _activityInstance.FormInstanceId.Value });

                            cmd.ExecuteNonQuery();

                            return _activityInstance.FormInstanceId.Value;
                        }
                        else
                        {
                            return Guid.Empty;
                        }
                    }
                }
            }
            else
            {
                return formInstanceId;
            }
        }

        public List<ActivityInstanceSummary> GetSummaryLog(Guid? refId, int flowInstanceId = 0)
        {
            List<ActivityInstanceSummary> activityLog = new List<ActivityInstanceSummary>();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ActivityInstanceSummaryLog", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@ActivityInstanceRefId", SqlDbType.UniqueIdentifier) { Value = refId.HasValue ? refId.Value : Guid.Empty });
                cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Value = flowInstanceId });
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    ExpandoObjectConverter expandoConverter = new ExpandoObjectConverter();
                    while (reader.Read())
                    {
                        activityLog.Add(new ActivityInstanceSummary()
                        {
                            RefId = reader.GetGuid(0),
                            ActivityName = reader.GetString(1),
                            UserName = reader.GetString(2),
                            Start = reader.GetDateTime(3),
                            End = reader.IsDBNull(4) ? new DateTime() : reader.GetDateTime(4)
                        });
                    }
                }
            }

            return activityLog;
        }

        public Guid Create(string activityId, int flowInstanceId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_Activity", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@ActivityId", SqlDbType.VarChar, 50) { Value = activityId });
                    cmd.Parameters.Add(new SqlParameter("@UserGroupTypeId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    if ((int)cmd.Parameters["@UserGroupTypeId"].Value == 1)
                    {
                        cmd.CommandText = "EXECUTION.usp_Get_UserGroup";
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Value = flowInstanceId });

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Create(reader.GetGuid(0), activityId, flowInstanceId, Guid.Empty);
                            }
                        }
                    }
                    else
                    {
                        return Create(UserId, activityId, flowInstanceId, Guid.Empty);
                    }
                }
            }

            return Guid.Empty;
        }

        public void Create(Guid actionInstanceId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ActivityActionInstance", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@ActionInstanceId", SqlDbType.UniqueIdentifier) { Value = actionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ActivityId", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@UserGroupTypeId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    string activityId = (string)cmd.Parameters["@ActivityId"].Value;
                    int flowInstanceId = (int)cmd.Parameters["@FlowInstanceId"].Value;

                    if ((int)cmd.Parameters["@UserGroupTypeId"].Value == 1)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "EXECUTION.usp_Get_UserGroup";
                        cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Value = flowInstanceId });

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Create(reader.GetGuid(0), activityId, flowInstanceId, actionInstanceId);
                            }
                        }
                    }
                    else
                    {
                        Create(Guid.Empty, activityId, flowInstanceId, actionInstanceId);
                    }
                }
            }
        }

        private Guid Create(Guid userId, string activityId, int flowInstanceId, Guid actionInstanceId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_ActivityInstance", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@ActivityId", SqlDbType.VarChar, 50) { Value = activityId });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Value = flowInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ActionInstanceId", SqlDbType.UniqueIdentifier) { Value = actionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@GetFormInstanceKey", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    int activityInstanceId = (int)cmd.Parameters["@ActivityInstanceId"].Value;

                    if ((string)cmd.Parameters["@GetFormInstanceKey"].Value == "auto")
                    {
                        ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(Configuration, UserId, Token);
                        actionInstanceHelper.Execute(userId, activityInstanceId);
                    }
                    else
                    {
                        MessageHelper messageHelper = new MessageHelper(Configuration, UserId, Token);
                        messageHelper.Create(activityInstanceId);
                    }
                        
                    return (Guid)cmd.Parameters["@ActivityInstanceRefId"].Value;
                }
            }
        }

        public void SetComment(Guid activityInstanceRefId, string comment)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_Comment", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceRefId", SqlDbType.UniqueIdentifier) { Value = activityInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@Comment", SqlDbType.VarChar, 255) { Value = comment });
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Comment> GetComments(Guid activityInstanceRefId)
        {
            List<Comment> comments = new List<Comment>();
            string scn = Configuration["CnDbTracking"];
            using (SqlConnection cn = new SqlConnection(scn))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_Comments", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                cmd.Parameters.Add(new SqlParameter("@ActivityInstanceRefId", SqlDbType.UniqueIdentifier) { Value = activityInstanceRefId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        comments.Add(new Comment()
                        {
                            Id = reader.GetGuid(0),
                            UserName = reader.GetString(1),
                            Text = reader.GetString(2),
                            Created = reader.GetDateTime(3),
                            IsMe = reader.GetBoolean(4)
                        });
                    }
                }
            }

            return comments;
        }

    }
}
