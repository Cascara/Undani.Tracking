﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Dynamic;
using System.Text;

namespace Undani.Tracking.Execution.Core
{
    public class ProcedureInstanceHelper : Helper
    {
        public ProcedureInstanceHelper(IConfiguration configuration, Guid userId, string token = "") : base(configuration, userId, token) { }

        public ProcedureInstanceCreated Create(Guid procedureRefId, Guid? systemActionInstanceId = null)
        {
            int procedureId = 0;
            ProcedureInstanceCreated procedureInstanceCreated = Unique(procedureRefId, ref procedureId);

            if (procedureInstanceCreated.ProcedureInstanceRefId == Guid.Empty)
            {
                int procedureInstanceId = 0;
                int flowId = 0;
                using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
                {
                    cn.Open();

                    using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_ProcedureInstance", cn) { CommandType = CommandType.StoredProcedure })
                    {

                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                        cmd.Parameters.Add(new SqlParameter("@ProcedureId", SqlDbType.Int) { Value = procedureId });
                        cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId ?? Guid.Empty });
                        cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                        cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                        cmd.Parameters.Add(new SqlParameter("@FlowId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                        cmd.ExecuteNonQuery();

                        procedureInstanceId = (int)cmd.Parameters["@ProcedureInstanceId"].Value;
                        procedureInstanceCreated.ProcedureInstanceRefId = (Guid)cmd.Parameters["@ProcedureInstanceRefId"].Value;
                        flowId = (int)cmd.Parameters["@FlowId"].Value;
                    }
                }

                procedureInstanceCreated.ElementInstanceRefId = new FlowInstanceHelper(Configuration, UserId, Token).Create(flowId, procedureInstanceId, systemActionInstanceId);
            }

            return procedureInstanceCreated;
        }

        private ProcedureInstanceCreated Unique(Guid procedureRefId, ref int procedureId)
        {
            ProcedureInstanceCreated procedureInstanceCreated = new ProcedureInstanceCreated();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureUnique", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureRefId", SqlDbType.UniqueIdentifier) { Value = procedureRefId });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ElementInstanceRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    procedureId = (int)cmd.Parameters["@ProcedureId"].Value;
                    procedureInstanceCreated.ProcedureInstanceRefId = (Guid)cmd.Parameters["@ProcedureInstanceRefId"].Value;
                    procedureInstanceCreated.ElementInstanceRefId = (Guid)cmd.Parameters["@ElementInstanceRefId"].Value;

                    return procedureInstanceCreated;
                }
            }
        }

        public ProcedureInstance Get(Guid procedureInstanceRefId)
        {
            ProcedureInstance procedureInstance;
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Value = procedureInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count == 1)
                        {
                            DataRow dr = dt.Rows[0];
                            ExpandoObjectConverter expandoConverter = new ExpandoObjectConverter();
                            procedureInstance = new ProcedureInstance()
                            {
                                RefId = procedureInstanceRefId,
                                ProcedureName = (string)dr["ProcedureName"],
                                ProcedureKey = (string)dr["ProcedureKey"],
                                Key = (string)dr["Key"],
                                Content = JsonConvert.DeserializeObject<ExpandoObject>((string)dr["Content"], expandoConverter),
                                States = JsonConvert.DeserializeObject<ExpandoObject>((string)dr["States"], expandoConverter),
                                ActivityInstances = GetLog((int)dr["Id"]),
                                EnvironmentId = (Guid)dr["EnvironmentId"],
                                DocumentsSigned = JsonConvert.DeserializeObject<ExpandoObject>((string)dr["DocumentsSigned"], expandoConverter)
                            };

                            if (dr["StartDate"] != DBNull.Value)
                                procedureInstance.StartDate = (DateTime)dr["StartDate"];

                            if (dr["EndDate"] != DBNull.Value)
                                procedureInstance.EndDate = (DateTime)dr["EndDate"];
                        }
                        else
                            throw new Exception("Could not get the activity");
                    }
                }
            }

            return procedureInstance;
        }

        public List<ProcedureInstanceSummary> GetInProcess()
        {
            List<ProcedureInstanceSummary> procedures = new List<ProcedureInstanceSummary>();

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstanceInProcess", cn) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    ExpandoObjectConverter expandoConverter = new ExpandoObjectConverter();
                    while (reader.Read())
                    {
                        procedures.Add(new ProcedureInstanceSummary()
                        {
                            RefId = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            Key = reader.GetString(2),
                            Content = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(3), expandoConverter),
                            Start = reader.GetDateTime(4),
                            States = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(5), expandoConverter),
                            DocumentsSignedZiped = GetDocumentsSignedZiped(reader.GetString(6)),
                            DocumentsSigned = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(6), expandoConverter)
                        });
                    }
                }
            }

            return procedures;
        }

        //public List<ProcedureInstanceSummary> GetSupportInProcess(Guid ownerId, Guid environmentId)
        //{
        //    List<ProcedureInstanceSummary> procedures = new List<ProcedureInstanceSummary>();

        //    using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
        //    {
        //        cn.Open();

        //        SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_SupportProcedureInstanceInProcess", cn) { CommandType = CommandType.StoredProcedure };

        //        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
        //        cmd.Parameters.Add(new SqlParameter("@OwnerId", SqlDbType.UniqueIdentifier) { Value = ownerId });
        //        cmd.Parameters.Add(new SqlParameter("@EnvironmentId", SqlDbType.UniqueIdentifier) { Value = environmentId });

        //        using (SqlDataReader reader = cmd.ExecuteReader())
        //        {

        //            ExpandoObjectConverter expandoConverter = new ExpandoObjectConverter();
        //            while (reader.Read())
        //            {
        //                procedures.Add(new ProcedureInstanceSummary()
        //                {
        //                    RefId = reader.GetGuid(0),
        //                    Name = reader.GetString(1),
        //                    Key = reader.GetString(2),
        //                    Content = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(3), expandoConverter),
        //                    Start = reader.GetDateTime(4)
        //                });
        //            }
        //        }
        //    }

        //    return procedures;
        //}

        public int GetInProcessCount()
        {
            int inProcessCount = 0;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstanceInProcessCount", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                cmd.Parameters.Add(new SqlParameter("@Count", SqlDbType.Int) { Direction = ParameterDirection.Output });

                cmd.ExecuteNonQuery();

                inProcessCount = (int)cmd.Parameters["@Count"].Value;
            }

            return inProcessCount;
        }

        public List<ProcedureInstanceSummary> GetResolved()
        {
            List<ProcedureInstanceSummary> procedures = new List<ProcedureInstanceSummary>();

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstanceResolved", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    ExpandoObjectConverter expandoConverter = new ExpandoObjectConverter();
                    while (reader.Read())
                    {
                        procedures.Add(new ProcedureInstanceSummary()
                        {
                            RefId = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            Key = reader.GetString(2),
                            Content = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(3), expandoConverter),
                            Start = reader.GetDateTime(4),
                            End = reader.GetDateTime(5),
                            States = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(6), expandoConverter),
                            DocumentsSignedZiped = GetDocumentsSignedZiped(reader.GetString(7)),
                            DocumentsSigned = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(7), expandoConverter)
                    });
                    }
                }
            }

            return procedures;
        }

        public List<ProcedureInstanceSummary> GetToTransfer()
        {
            List<ProcedureInstanceSummary> procedures = new List<ProcedureInstanceSummary>();

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstanceToTransfer", cn) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    ExpandoObjectConverter expandoConverter = new ExpandoObjectConverter();
                    while (reader.Read())
                    {
                        procedures.Add(new ProcedureInstanceSummary()
                        {
                            RefId = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            Key = reader.GetString(2),
                            Content = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(3), expandoConverter),
                            Start = reader.GetDateTime(4)
                        });
                    }
                }
            }

            return procedures;
        }

        public List<ActivityInstanceSummary> GetLog(Guid procedureInstanceRefId)
        {
            List<ActivityInstanceSummary> activityLog = new List<ActivityInstanceSummary>();

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstanceRefIdLog", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Value = procedureInstanceRefId });
               
                return new ActivityInstanceHelper(Configuration, UserId, Token).FillActivitiesInstanceSummary(cmd);
            }
        }

        public List<ActivityInstanceSummary> GetSupportLog(Guid ownerId, Guid procedureInstanceRefId)
        {
            List<ActivityInstanceSummary> activityLog = new List<ActivityInstanceSummary>();

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_SupportProcedureInstanceRefIdLog", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                cmd.Parameters.Add(new SqlParameter("@OwnerId", SqlDbType.UniqueIdentifier) { Value = ownerId });
                cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Value = procedureInstanceRefId });

                return new ActivityInstanceHelper(Configuration, UserId, Token).FillActivitiesInstanceSummary(cmd);
            }
        }

        public List<ActivityInstanceSummary> GetLog(int procedureInstanceId)
        {
            List<ActivityInstanceSummary> activityLog = new List<ActivityInstanceSummary>();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstanceIdLog", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceId", SqlDbType.Int) { Value = procedureInstanceId });

                return new ActivityInstanceHelper(Configuration, UserId, Token).FillActivitiesInstanceSummary(cmd);
            }
        }

        public List<Comment> GetComments(Guid procedureInstanceRefId)
        {
            List<Comment> comments = new List<Comment>();
            string scn = Configuration["CnDbTracking"];
            using (SqlConnection cn = new SqlConnection(scn))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstanceComments", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Value = procedureInstanceRefId });

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

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_ProcedureInstanceComment", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ElementInstanceRefId", SqlDbType.UniqueIdentifier) { Value = elementInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@Comment", SqlDbType.VarChar, 255) { Value = comment });
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool SetState(Guid procedureInstanceRefId, string key, string state)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_StateProcedureInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Value = procedureInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = key });
                    cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 50) { Value = state });
                    cmd.ExecuteNonQuery();
                }
            }

            return true;
        }

        public dynamic GetState(Guid procedureInstanceRefId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_StateProcedureInstanceRefId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Value = procedureInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@States", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();

                    return JsonConvert.DeserializeObject<ExpandoObject>((string)cmd.Parameters["@States"].Value, new ExpandoObjectConverter());
                }
            }
        }

        public void SetDocumentsSigned(Guid procedureInstanceRefId, string key, DocumentSigned[] documentsSigned)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_ProcedureInstanceDocumentSigned", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Value = procedureInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = key });
                    cmd.Parameters.Add(new SqlParameter("@DocumentSigned", SqlDbType.VarChar, 500) { Value = JsonConvert.SerializeObject(documentsSigned) });

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool SetContentProperty(Guid procedureInstanceRefId, string propertyName, string value, string type)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_ProcedureInstanceRefIdContentProperty", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Value = procedureInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@PropertyName", SqlDbType.VarChar, 50) { Value = propertyName });
                    cmd.Parameters.Add(new SqlParameter("@Value", SqlDbType.VarChar, 1000) { Value = value });
                    cmd.Parameters.Add(new SqlParameter("@Type", SqlDbType.VarChar, 20) {Value = type });

                    cmd.ExecuteNonQuery();
                }
            }

            return true;
        }

        public bool Transfer(Guid procedureInstanceRefId, Guid destinyUserId, string key)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_ProcedureInstanceTransfer", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Value = procedureInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@DestinyUserId", SqlDbType.UniqueIdentifier) { Value = destinyUserId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 150) { Value = key });
                    cmd.ExecuteNonQuery();
                }
            }

            return true;
        }

        public bool TransferAll(Guid sourceUserId, Guid destinyUserId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_ProcedureInstanceTransferAll", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@SourceUserId", SqlDbType.UniqueIdentifier) { Value = sourceUserId });
                    cmd.Parameters.Add(new SqlParameter("@DestinyUserId", SqlDbType.UniqueIdentifier) { Value = destinyUserId });
                    cmd.ExecuteNonQuery();
                }
            }

            return true;
        }

        public List<UserSelected> GetUserSelected(Guid procedureInstanceRefId) 
        {
            List<UserSelected> userSelecteds = new List<UserSelected>();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstanceUserSelected", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Value = procedureInstanceRefId });

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userSelecteds.Add(new UserSelected()
                            {
                                ProcedureInstanceRefId = procedureInstanceRefId,
                                Key = reader.GetString(0),
                                UserId = reader.GetGuid(1),
                                GivenName = reader.GetString(2),
                                FamilyName = reader.GetString(3),
                                EMail = reader.GetString(4)
                            });
                        }
                    }
                }
            }

            return userSelecteds;
        }

    }
}
