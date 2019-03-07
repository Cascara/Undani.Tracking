using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Undani.JWT;
using Undani.Tracking.Execution.Core;
using Undani.Tracking.Execution.Core.Infra;

namespace Undani.Tracking.Execution.API.Controllers
{
    [Produces("application/json")]
    [Route("Execution")]
    public class ExecutionController : Controller
    {
        private IConfiguration _configuration;

        internal ExecutionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
               
        #region ProcedureInstance
        [Route("ProcedureInstance/")]
        public ProcedureInstance GetProcedureInstance(Guid procedureInstanceRefId)
        {
            Guid userId = GetUser(Request);
            return new ProcedureInstanceHelper(_configuration).Get(userId, procedureInstanceRefId);
        }

        [Route("ProcedureInstance/Create")]
        public Guid CreateProcedureInstance(Guid procedureRefId, Guid? activityInstanceRefId = null)
        {
            Guid userId = GetUser(Request);
            return new ProcedureInstanceHelper(_configuration).Create(userId, procedureRefId, activityInstanceRefId);
        }
        #endregion

        #region FlowInstance
        [Route("FlowInstance")]
        public FlowInstance GetFlowInstance(Guid flowInstanceRefId)
        {
            Guid userId = GetUser(Request);
            return new FlowInstanceHelper(_configuration).Get(userId, flowInstanceRefId);
        }
        
        [Route("FlowInstance/SetContentProperty")]
        public dynamic SetContentProperty(Guid flowInstanceRefId, string propertyName, string value)
        {
            Guid userId = GetUser(Request);
            return new FlowInstanceHelper(_configuration).SetContentProperty(userId, flowInstanceRefId, propertyName, value);
        }

        [Route("FlowInstance/SetContentProperty/ByFormInstance")]
        public dynamic SetContentPropertyFormInstance(Guid formInstanceId, string propertyName, string value)
        {
            Guid userId = GetUser(Request);
            return new FlowInstanceHelper(_configuration).SetContentPropertyFormInstance(userId, formInstanceId, propertyName, value);
        }

        [HttpPost]
        [Route("FlowInstance/SetUserGroup")]
        public void SetUserGroup(Guid flowInstanceRefId, [FromBody] UserGroup[] users)
        {
            Guid userId = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(_configuration);
            flowInstanceHelper.SetUserGroup(userId, flowInstanceRefId, users);
        }

        [HttpPost]
        [Route("FlowInstance/SetUserGroup/ByFormInstance")]
        public void SetUserGroupFormInstance(Guid formInstanceId, [FromBody] UserGroup[] users)
        {
            Guid userId = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(_configuration);
            flowInstanceHelper.SetUserGroupFormInstance(userId, formInstanceId, users);
        }

        [Route("FlowInstance/SetState")]
        public void SetState(Guid activityInstanceRefId, string key, string state)
        {
            Guid userId = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(_configuration);
            flowInstanceHelper.SetState(userId, activityInstanceRefId, key, state);
        }

        [Route("FlowInstance/SetState/ByFormInstance")]
        public void SetStateFormInstance(Guid formInstanceId, string key, string state)
        {
            Guid userId = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(_configuration);
            flowInstanceHelper.SetStateFormInstance(userId, formInstanceId, key, state);
        }

        [Route("FlowInstance/GetLog")]
        public PagedList<FlowInstanceSummary> GetFlowLog(int? pageLimit = null, int? page = null)
        {
            Guid userId = GetUser(Request);
            return new FlowInstanceHelper(_configuration).GetLog(userId, pageLimit, page);
        }
        #endregion

        #region Message  
        [Route("Message/GetOpen")]
        public OpenedMessage GetMessageOpen(Guid messageId)
        {
            Guid userId = GetUser(Request);
            return new MessageHelper(_configuration).GetOpen(userId, messageId);
        }

        [Route("Message/GetReceived")]
        public List<Message> GetMessagesReceived()
        {
            Guid userId = GetUser(Request);
            return new MessageHelper(_configuration).GetReceived(userId);
        }

        [Route("Message/GetDrafts")]
        public List<Message> GetMessagesDrafts()
        {
            Guid userId = GetUser(Request);
            return new MessageHelper(_configuration).GetDrafts(userId);
        }
        #endregion

        #region Activity 
        [Route("ActivityInstance")]
        public ActivityInstance GetActivityInstance(Guid activityInstanceRefId)
        {
            Guid userId = GetUser(Request);
            return new ActivityInstanceHelper(_configuration).Get(userId, activityInstanceRefId);
        }

        [Route("ActivityInstance/GetLog")]
        public List<ActivityInstanceSummary> GetActivityInstanceLog(Guid activityInstanceRefId)
        {
            Guid userId = GetUser(Request);
            return new ActivityInstanceHelper(_configuration).GetSummaryLog(userId, activityInstanceRefId); ;
        }

        [Route("ActivityInstance/SetComment")]
        public string SetComment(Guid activityInstanceRefId, string comment)
        {
            Guid userId = GetUser(Request);
            ActivityInstanceHelper activityInstanceHelper = new ActivityInstanceHelper(_configuration);
            activityInstanceHelper.SetComment(userId, activityInstanceRefId, comment);
            return comment;
        }

        [Route("ActivityInstance/GetComments")]
        public List<Comment> GetComments(Guid activityInstanceRefId)
        {
            Guid userId = GetUser(Request);
            return new ActivityInstanceHelper(_configuration).GetComments(userId, activityInstanceRefId); ;
        }
        #endregion

        #region Action 
        [Route("ActionInstance/Execute")]
        public void ExecuteActionInstance(Guid actionRefId, Guid activityInstanceRefId)
        {
            Guid userId = GetUser(Request);
            ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(_configuration);
            actionInstanceHelper.Execute(userId, actionRefId, activityInstanceRefId);
        }

        [Route("ActionInstance/Execute/ByFormInstance")]
        public void ExecuteActionInstanceFormInstance(Guid formInstanceId)
        {
            Guid userId = GetUser(Request);
            ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(_configuration);
            actionInstanceHelper.Execute(userId, formInstanceId);
        }

        [Route("SystemAccionInstance/Finish")]
        public void FinishSystemAction(Guid systemActionInstanceId)
        {
            SystemActionInstanceHelper systemActionInstanceHelper = new SystemActionInstanceHelper(_configuration);
            systemActionInstanceHelper.Finish(systemActionInstanceId);
        }

        [Route("SystemAccionInstance/Execute")]
        public void ExecuteSystemAction(Guid systemActionInstanceId)
        {
            SystemActionInstanceHelper systemActionInstanceHelper = new SystemActionInstanceHelper(_configuration);
            systemActionInstanceHelper.Execute(systemActionInstanceId);
        }
        #endregion

        #region Tools   
        private Guid GetUser(HttpRequest request)
        {
            Payload payload = new Payload();
            if (!request.Headers.ContainsKey("Authorization"))
                throw new OperationCanceledException("Invalid access.");

            var authHeader = request.Headers["Authorization"][0];
            if (authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length);

                try
                {
                    payload = JWToken.TokenDecode(token);
                }
                catch (Exception e)
                {
                    throw new OperationCanceledException("Invalid access.");
                }
            }

            return  Guid.Parse(payload.UserId);
        }
        #endregion

    }
}