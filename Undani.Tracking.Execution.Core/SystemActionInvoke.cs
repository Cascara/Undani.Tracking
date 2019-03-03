//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.Reflection;
//using System.Text;

//namespace Undani.Tracking.Invoke
//{
//    public static partial class SystemActionInvoke
//    {
//        private static void Start(bool start, Guid systemActionInstanceId)
//        {
//            if (start)
//            {
//                using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
//                {
//                    cn.Open();

//                    using (SqlCommand cmd = new SqlCommand("sp_Start_SystemActionInstance", cn))
//                    {
//                        cmd.CommandType = CommandType.StoredProcedure;
//                        cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });

//                        cmd.ExecuteNonQuery();
//                    }
//                }
//            }
//        }

//        public static void Invoke(Guid systemActionInstanceId, string method, string alias, string configuration)
//        {
//            MethodInfo methodInfo = typeof(SystemActionInvoke).GetMethod(method);

//            bool start = Convert.ToBoolean(methodInfo.Invoke(null, new object[] { systemActionInstanceId, alias, configuration }));

//            //SystemActionInvoke

//            if (start)
//                Start(start, systemActionInstanceId);
//        }
//    }
//}
