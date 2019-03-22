﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Undani.Tracking.Execution.Core.Resource;

namespace Undani.Tracking.Execution.Core
{
    public class ActionInstanceHelper : Helper
    {
        public ActionInstanceHelper(IConfiguration configuration, Guid userId, string token = "") : base(configuration, userId, token) { }

        /// <summary>
        /// It allows to get action from a especific activity
        /// </summary>
        /// <param name="activityId">Unique identifier of activity</param>
        /// <returns>List of actions</returns>
        internal List<ActionButton> GetActions(string activityId)
        {
            List<ActionButton> actions = new List<ActionButton>();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_Actions", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@ActivitySourceId", SqlDbType.VarChar, 50) { Value = activityId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        actions.Add(new ActionButton()
                        {
                            RefId = reader.GetGuid(0),
                            Key = reader.GetString(1),
                            Name = reader.GetString(2),
                            Description = reader.GetString(3),
                            SignatureType = reader.GetString(4),
                            Color = reader.GetString(5),
                            Icon = reader.GetString(6),
                            Order = reader.GetInt32(7)
                        });
                    }
                }
            }

            return actions;
        }

        public void Execute(int activityInstanceId)
        {
            string actionId;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ActionActivityInstanceId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceId", SqlDbType.Int) { Value = activityInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ActionId", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    actionId = (string)cmd.Parameters["@ActionId"].Value;
                }
            }

            Create(actionId, activityInstanceId);
        }

        public void Execute(Guid actionRefId, Guid activityInstanceRefId)
        {
            string actionId;
            int activityInstanceId;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ActionActivityInstanceRefId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ActionRefId", SqlDbType.UniqueIdentifier) { Value = actionRefId });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceRefId", SqlDbType.UniqueIdentifier) { Value = activityInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@ActionId", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    actionId = (string)cmd.Parameters["@ActionId"].Value;
                    activityInstanceId = (int)cmd.Parameters["@ActivityInstanceId"].Value;
                }
            }

            Create(actionId, activityInstanceId);
        }

        public void Execute(Guid actionRefId, int activityInstanceId)
        {
            string actionId;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_Action", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ActionRefId", SqlDbType.UniqueIdentifier) { Value = actionRefId });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceId", SqlDbType.Int) { Value = activityInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ActionId", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    actionId = (string)cmd.Parameters["@ActionId"].Value;
                }
            }

            Create(actionId, activityInstanceId);
        }

        private void Create(string actionId, int activityInstanceId)
        {
            Guid actionInstanceId;
            Guid formInstanceId;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))   
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_ActionInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ActionId", SqlDbType.VarChar, 50) { Value = actionId });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceId", SqlDbType.Int) { Value = activityInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ActionInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    actionInstanceId = (Guid)cmd.Parameters["@ActionInstanceId"].Value;
                    formInstanceId = (Guid)cmd.Parameters["@FormInstanceId"].Value;

                    if (formInstanceId != Guid.Empty)
                    {
                        FormCall formCall = new FormCall(Configuration);
                        formCall.SetReadOnly(formInstanceId, Token);
                    }

                    if (cmd.Parameters["@SystemActionInstanceId"].Value != DBNull.Value)
                    {
                        SystemActionInstanceHelper systemActionInstanceHelper = new SystemActionInstanceHelper(Configuration, UserId, Token);
                        systemActionInstanceHelper.Execute((Guid)cmd.Parameters["@SystemActionInstanceId"].Value);
                    }
                    else
                    {
                        Finish(actionInstanceId);
                    }
                }
            }            
        }

        public void Finish(Guid actionInstanceId)
        {
            ActivityInstanceHelper activityInstanceHelper = new ActivityInstanceHelper(Configuration, UserId, Token);
            activityInstanceHelper.Create(actionInstanceId);
        }
    }
}
