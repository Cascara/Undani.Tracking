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
            var user = GetUser(Request);
            return new ProcedureInstanceHelper(_configuration).Get(user.UserId, procedureInstanceRefId);
        }

        [Route("ProcedureInstance/Create")]
        public Guid CreateProcedureInstance(Guid procedureRefId, Guid? activityInstanceRefId = null)
        {
            var user = GetUser(Request);
            return new ProcedureInstanceHelper(_configuration).Create(user.UserId, user.OwnerId, procedureRefId, activityInstanceRefId);
        }
        #endregion

        #region FlowInstance
        [Route("FlowInstance")]
        public FlowInstance GetFlowInstance(Guid flowInstanceRefId)
        {
            var user = GetUser(Request);
            return new FlowInstanceHelper(_configuration).Get(user.UserId, flowInstanceRefId);
        }
        
        [Route("FlowInstance/SetContentProperty")]
        public dynamic SetContentProperty(Guid flowInstanceRefId, string propertyName, string value)
        {
            var user = GetUser(Request);
            return new FlowInstanceHelper(_configuration).SetContentProperty(user.UserId, flowInstanceRefId, propertyName, value);
        }

        [Route("FlowInstance/SetContentProperty/ByFormInstance")]
        public dynamic SetContentPropertyFormInstance(Guid formInstanceId, string propertyName, string value)
        {
            var user = GetUser(Request);
            return new FlowInstanceHelper(_configuration).SetContentPropertyFormInstance(user.UserId, formInstanceId, propertyName, value);
        }

        [HttpPost]
        [Route("FlowInstance/SetUserGroup")]
        public void SetUserGroup(Guid flowInstanceRefId, [FromBody] UserGroup[] users)
        {
            var user = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(_configuration);
            flowInstanceHelper.SetUserGroup(user.UserId, flowInstanceRefId, users);
        }

        [HttpPost]
        [Route("FlowInstance/SetUserGroup/ByFormInstance")]
        public void SetUserGroupFormInstance(Guid formInstanceId, [FromBody] UserGroup[] users)
        {
            var user = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(_configuration);
            flowInstanceHelper.SetUserGroupFormInstance(user.UserId, formInstanceId, users);
        }

        [Route("FlowInstance/SetState")]
        public void SetState(Guid activityInstanceRefId, string key, string state)
        {
            var user = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(_configuration);
            flowInstanceHelper.SetState(user.UserId, activityInstanceRefId, key, state);
        }

        [Route("FlowInstance/SetState/ByFormInstance")]
        public void SetStateFormInstance(Guid formInstanceId, string key, string state)
        {
            var user = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(_configuration);
            flowInstanceHelper.SetStateFormInstance(user.UserId, formInstanceId, key, state);
        }

        [Route("FlowInstance/GetLog")]
        public PagedList<FlowInstanceSummary> GetFlowLog(int? pageLimit = null, int? page = null)
        {
            var user = GetUser(Request);
            return new FlowInstanceHelper(_configuration).GetLog(user.UserId, pageLimit, page);
        }
        #endregion

        #region Message  
        [Route("Message/GetOpen")]
        public OpenedMessage GetMessageOpen(Guid messageId)
        {
            var user = GetUser(Request);
            return new MessageHelper(_configuration).GetOpen(user.UserId, messageId);
        }

        [Route("Message/GetReceived")]
        public List<Message> GetMessagesReceived()
        {
            var user = GetUser(Request);
            return new MessageHelper(_configuration).GetReceived(user.UserId);
        }

        [Route("Message/GetDrafts")]
        public List<Message> GetMessagesDrafts()
        {
            var user = GetUser(Request);
            return new MessageHelper(_configuration).GetDrafts(user.UserId);
        }
        #endregion

        #region Activity 
        [Route("ActivityInstance")]
        public ActivityInstance GetActivityInstance(Guid activityInstanceRefId)
        {
            var user = GetUser(Request);
            return new ActivityInstanceHelper(_configuration).Get(user.UserId, activityInstanceRefId);
        }

        [Route("ActivityInstance/GetLog")]
        public List<ActivityInstanceSummary> GetActivityInstanceLog(Guid activityInstanceRefId)
        {
            var user = GetUser(Request);
            return new ActivityInstanceHelper(_configuration).GetSummaryLog(user.UserId, activityInstanceRefId); ;
        }

        [Route("ActivityInstance/SetComment")]
        public string SetComment(Guid activityInstanceRefId, string comment)
        {
            var user = GetUser(Request);            
            return new ActivityInstanceHelper(_configuration).SetComment(user.UserId, activityInstanceRefId, comment);
        }

        [Route("ActivityInstance/GetComments")]
        public List<Comment> GetComments(Guid activityInstanceRefId)
        {
            var user = GetUser(Request);
            return new ActivityInstanceHelper(_configuration).GetComments(user.UserId, activityInstanceRefId); ;
        }
        #endregion

        #region Action 
        [Route("ActionInstance/Execute")]
        public void ExecuteActionInstance(Guid actionRefId, Guid activityInstanceRefId)
        {
            var user = GetUser(Request);
            ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(_configuration);
            actionInstanceHelper.Execute(user.UserId, actionRefId, activityInstanceRefId);
        }

        [Route("ActionInstance/Execute/ByFormInstance")]
        public void ExecuteActionInstanceFormInstance(Guid formInstanceId)
        {
            var user = GetUser(Request);
            ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(_configuration);
            actionInstanceHelper.Execute(user.UserId, formInstanceId);
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
        private _User GetUser(HttpRequest request)
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

            if(payload.Owners)

            return  new _User() { UserId = Guid.Parse(payload.UserId), OwnerId = Guid.Parse(_configuration.GetValue("Owner:" + host)) };
        }
        #endregion

    }

    internal class _User
    {
        public Guid UserId { get; set; }
        public Guid OwnerId { get; set; }
    }
}