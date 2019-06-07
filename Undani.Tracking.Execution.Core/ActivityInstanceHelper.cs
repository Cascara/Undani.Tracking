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

        public string IsAnonymous(Guid elementInstanceRefId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ActivityInstanceIsAnonymous", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ElementInstanceRefId", SqlDbType.UniqueIdentifier) { Value = elementInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@IsAnonymous", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();

                    if ((bool)cmd.Parameters["@IsAnonymous"].Value)
                        return Token;
                    else
                        return "";
                }
            }
        }

        public ActivityInstance Get(Guid elementInstanceRefId)
        {
            ActivityInstance activity;
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ActivityInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ElementInstanceRefId", SqlDbType.UniqueIdentifier) { Value = elementInstanceRefId });

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count == 1)
                        {
                            DataRow dr = dt.Rows[0];
                            activity = new ActivityInstance()
                            {
                                RefId = elementInstanceRefId,
                                Name = (string)dr["ElementName"],
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
                                activity.ActionButtons = new ActionInstanceHelper(Configuration, UserId, Token).GetActions((string)dr["ElementId"]);
                        }
                        else
                            throw new Exception("Could not get the activity");
                    }
                }
            }

            return activity;
        }

        private Guid SetFormInstance(Guid formInstanceId, int elementInstanceId)
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
                        cmd.Parameters.Add(new SqlParameter("@ElementInstanceId", SqlDbType.Int) { Value = elementInstanceId });
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
                            cmd.Parameters.Add(new SqlParameter("@ElementInstanceId", SqlDbType.Int) { Value = elementInstanceId });
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

        public List<ActivityInstanceSummary> GetLogFlowInstance(int flowInstanceId)
        {
            List<ActivityInstanceSummary> activityLog = new List<ActivityInstanceSummary>();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ActivityInstanceFlowInstanceLog", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Value = flowInstanceId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        activityLog.Add(new ActivityInstanceSummary()
                        {
                            RefId = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            CoustomViewer = reader.GetString(2),
                            UserId = reader.GetGuid(3),
                            UserName = reader.GetString(4),
                            Start = reader.GetDateTime(5),
                            End = reader.IsDBNull(6) ? new DateTime() : reader.GetDateTime(5),
                            Days = reader.GetString(7),
                            Hours = reader.GetString(8),
                            Reference = reader.GetString(9)
                        });
                    }
                }
            }

            return activityLog;
        }

        public List<ActivityInstanceSummary> GetLogProcedureInstance(int procedureInstanceId)
        {
            List<ActivityInstanceSummary> activityLog = new List<ActivityInstanceSummary>();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ActivityInstanceProcedureInstanceLog", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceId", SqlDbType.Int) { Value = procedureInstanceId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        activityLog.Add(new ActivityInstanceSummary()
                        {
                            RefId = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            CoustomViewer = reader.GetString(2),
                            UserId = reader.GetGuid(3),
                            UserName = reader.GetString(4),
                            Start = reader.GetDateTime(5),
                            End = reader.IsDBNull(6) ? new DateTime() : reader.GetDateTime(5),
                            Days = reader.GetString(7),
                            Hours = reader.GetString(8),
                            Reference = reader.GetString(9)
                        });
                    }
                }
            }

            return activityLog;
        }

        public void Create(string elementId,int elementInstanceId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_ActivityInstance", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@ElementId", SqlDbType.VarChar, 50) { Value = elementId });
                    cmd.Parameters.Add(new SqlParameter("@ElementInstanceId", SqlDbType.Int) { Value = elementInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@GetFormInstanceKey", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    if ((string)cmd.Parameters["@GetFormInstanceKey"].Value == "auto")
                    {
                        ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(Configuration, UserId, Token);
                        actionInstanceHelper.Execute(UserId, elementInstanceId);
                    }
                    else
                    {
                        MessageHelper messageHelper = new MessageHelper(Configuration, UserId, Token);
                        messageHelper.Create(elementInstanceId);
                    }
                }
            }
        }

        public List<Comment> GetComments(Guid elementInstanceRefId)
        {
            List<Comment> comments = new List<Comment>();
            string scn = Configuration["CnDbTracking"];
            using (SqlConnection cn = new SqlConnection(scn))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_Comments", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                cmd.Parameters.Add(new SqlParameter("@ElementInstanceRefId", SqlDbType.UniqueIdentifier) { Value = elementInstanceRefId });

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

        public void SetComment(Guid elementInstanceRefId, string comment)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_Comment", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ElementInstanceRefId", SqlDbType.UniqueIdentifier) { Value = elementInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@Comment", SqlDbType.VarChar, 255) { Value = comment });
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public ActivityInstanceSignature GetSignature(Guid elementInstanceRefId)
        {
            ActivityInstanceSignature activityInstanceSignature = new ActivityInstanceSignature() { RefId = elementInstanceRefId, FormInstanceId = Guid.Empty };
            string scn = Configuration["CnDbTracking"];
            using (SqlConnection cn = new SqlConnection(scn))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ElementSignature", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@ElementInstanceRefId", SqlDbType.UniqueIdentifier) { Value = elementInstanceRefId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        activityInstanceSignature.ElementsSignatures.Add(new ElementSignature()
                        {                            
                            Template = reader.GetString(1),
                            Key = reader.GetString(2),
                            JsonPaths = reader.GetString(3).Split(',').ToList(),
                            ElementSignatureTypeId = reader.GetInt32(4),
                            Content = reader.GetString(5),
                            OriginalName = reader.GetString(6),
                            Create = reader.GetBoolean(7)
                        });

                        if (activityInstanceSignature.FormInstanceId == Guid.Empty)
                        {
                            activityInstanceSignature.ElementId = reader.GetString(0);
                            activityInstanceSignature.FormInstanceId = reader.GetGuid(8);
                            activityInstanceSignature.EnvironmentId = reader.GetGuid(9);
                            activityInstanceSignature.ProcedureInstanceRefId = reader.GetGuid(10);
                        }
                            
                    }
                }
            }

            return activityInstanceSignature;
        }

        public ActivityInstanceSignature GetSignatureTemplate(Guid elementInstanceRefId, string template)
        {
            ActivityInstanceSignature activityInstanceSignature = new ActivityInstanceSignature() { RefId = elementInstanceRefId, FormInstanceId = Guid.Empty };
            string scn = Configuration["CnDbTracking"];
            using (SqlConnection cn = new SqlConnection(scn))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ElementSignatureTemplate", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@ElementInstanceRefId", SqlDbType.UniqueIdentifier) { Value = elementInstanceRefId });
                cmd.Parameters.Add(new SqlParameter("@Template", SqlDbType.VarChar, 50) { Value = template });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        activityInstanceSignature.ElementsSignatures.Add(new ElementSignature()
                        {
                            Template = reader.GetString(1),
                            Key = reader.GetString(2),
                            JsonPaths = reader.GetString(3).Split(',').ToList(),
                            ElementSignatureTypeId = reader.GetInt32(4),
                            Content = reader.GetString(5),
                            OriginalName = reader.GetString(6),
                            Create = reader.GetBoolean(7)
                        });

                        if (activityInstanceSignature.FormInstanceId == Guid.Empty)
                        {
                            activityInstanceSignature.ElementId = reader.GetString(0);
                            activityInstanceSignature.FormInstanceId = reader.GetGuid(8);
                            activityInstanceSignature.EnvironmentId = reader.GetGuid(9);
                            activityInstanceSignature.ProcedureInstanceRefId = reader.GetGuid(10);
                        }

                    }
                }
            }

            return activityInstanceSignature;
        }

        public void SetDocumentSigned(Guid elementInstanceRefId, string key, DocumentSigned documentSigned)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_DocumentSigned", cn) { CommandType = CommandType.StoredProcedure })
                {                    
                    cmd.Parameters.Add(new SqlParameter("@ElementInstanceRefId", SqlDbType.UniqueIdentifier) { Value = elementInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = key });
                    cmd.Parameters.Add(new SqlParameter("@DocumentSigned", SqlDbType.VarChar, 250) { Value = JsonConvert.SerializeObject(documentSigned) });

                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}
