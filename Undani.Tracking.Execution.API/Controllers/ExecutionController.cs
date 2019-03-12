using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Undani.JWT;
using Undani.Tracking.Execution.API.Infra;
using Undani.Tracking.Execution.Core;
using Undani.Tracking.Execution.Core.Infra;

namespace Undani.Tracking.Execution.API.Controllers
{
    [Produces("application/json")]
    [Route("Execution")]
    public class ExecutionController : Controller
    {
        private IConfiguration _configuration;

        public ExecutionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
               
        #region ProcedureInstance
        [Route("ProcedureInstance/")]
        public ProcedureInstance GetProcedureInstance(Guid procedureInstanceRefId)
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(_configuration, user.UserId, user.Token).Get(procedureInstanceRefId);
        }

        [Route("ProcedureInstance/Create")]
        public Guid CreateProcedureInstance(Guid procedureRefId, Guid? activityInstanceRefId = null)
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(_configuration, user.UserId, user.Token).Create(procedureRefId, activityInstanceRefId);
        }

        [Route("ProcedureInstance/Create/Anonymous")]
        public Guid CreateAnonymousProcedureInstance(Guid procedureRefId, Guid? activityInstanceRefId = null)
        {
            return new ProcedureInstanceHelper(_configuration, Guid.Empty).Create(procedureRefId, activityInstanceRefId);
        }
        #endregion

        #region FlowInstance
        [Route("FlowInstance")]
        public FlowInstance GetFlowInstance(Guid flowInstanceRefId)
        {
            _User user = GetUser(Request);
            return new FlowInstanceHelper(_configuration, user.UserId, user.Token).Get(flowInstanceRefId);
        }
        
        [Route("FlowInstance/SetContentProperty")]
        public dynamic SetContentProperty(Guid flowInstanceRefId, string propertyName, string value)
        {
            _User user = GetUser(Request);
            return new FlowInstanceHelper(_configuration, user.UserId, user.Token).SetContentProperty(flowInstanceRefId, propertyName, value);
        }

        [Route("FlowInstance/SetContentProperty/ByFormInstance")]
        public dynamic SetContentPropertyFormInstance(Guid formInstanceId, string propertyName, string value)
        {
            _User user = GetUser(Request);
            return new FlowInstanceHelper(_configuration, user.UserId, user.Token).SetContentPropertyFormInstance(formInstanceId, propertyName, value);
        }

        [HttpPost]
        [Route("FlowInstance/SetUserGroup")]
        public void SetUserGroup(Guid flowInstanceRefId, [FromBody] UserGroup[] users)
        {
            _User user = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(_configuration, user.UserId, user.Token);
            flowInstanceHelper.SetUserGroup(flowInstanceRefId, users);
        }

        [HttpPost]
        [Route("FlowInstance/SetUserGroup/ByFormInstance")]
        public void SetUserGroupFormInstance(Guid formInstanceId, [FromBody] UserGroup[] users)
        {
            _User user = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(_configuration, user.UserId, user.Token);
            flowInstanceHelper.SetUserGroupFormInstance(formInstanceId, users);
        }

        [Route("FlowInstance/SetState")]
        public void SetState(Guid activityInstanceRefId, string key, string state)
        {
            _User user = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(_configuration, user.UserId, user.Token);
            flowInstanceHelper.SetState(activityInstanceRefId, key, state);
        }

        [Route("FlowInstance/SetState/ByFormInstance")]
        public void SetStateFormInstance(Guid formInstanceId, string key, string state)
        {
            _User user = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(_configuration, user.UserId, user.Token);
            flowInstanceHelper.SetStateFormInstance(formInstanceId, key, state);
        }

        [Route("FlowInstance/GetLog")]
        public PagedList<FlowInstanceSummary> GetFlowLog(int? pageLimit = null, int? page = null)
        {
            _User user = GetUser(Request);
            return new FlowInstanceHelper(_configuration, user.UserId, user.Token).GetLog(pageLimit, page);
        }
        #endregion

        #region Message  
        [Route("Message/GetOpen")]
        public OpenedMessage GetMessageOpen(Guid messageId)
        {
            _User user = GetUser(Request);
            return new MessageHelper(_configuration, user.UserId, user.Token).GetOpen(messageId);
        }

        [Route("Message/GetReceived")]
        public List<Message> GetMessagesReceived()
        {
            _User user = GetUser(Request);
            return new MessageHelper(_configuration, user.UserId, user.Token).GetReceived();
        }

        [Route("Message/GetDrafts")]
        public List<Message> GetMessagesDrafts()
        {
            _User user = GetUser(Request);
            return new MessageHelper(_configuration, user.UserId, user.Token).GetDrafts();
        }
        #endregion

        #region Activity 
        [Route("ActivityInstance")]
        public ActivityInstance GetActivityInstance(Guid activityInstanceRefId)
        {
            _User user = GetUser(Request);
            return new ActivityInstanceHelper(_configuration, user.UserId, user.Token).Get(activityInstanceRefId);
        }

        [Route("ActivityInstance/Anonymous")]
        public ActivityInstance GetAnonymousActivityInstance(Guid activityInstanceRefId)
        {
            return new ActivityInstanceHelper(_configuration, Guid.Empty).Get(activityInstanceRefId);
        }

        [Route("ActivityInstance/IsAnonymous")]
        public string GetIsAnonymousActivityInstance(Guid activityInstanceRefId)
        {
            return new ActivityInstanceHelper(_configuration, Guid.Empty).IsAnonymous(activityInstanceRefId);
        }

        [Route("ActivityInstance/GetLog")]
        public List<ActivityInstanceSummary> GetActivityInstanceLog(Guid activityInstanceRefId)
        {
            _User user = GetUser(Request);
            return new ActivityInstanceHelper(_configuration, user.UserId, user.Token).GetSummaryLog(activityInstanceRefId);
        }

        [Route("ActivityInstance/SetComment")]
        public string SetComment(Guid activityInstanceRefId, string comment)
        {
            _User user = GetUser(Request);
            ActivityInstanceHelper activityInstanceHelper = new ActivityInstanceHelper(_configuration, user.UserId, user.Token);
            activityInstanceHelper.SetComment(activityInstanceRefId, comment);
            return comment;
        }

        [Route("ActivityInstance/GetComments")]
        public List<Comment> GetComments(Guid activityInstanceRefId)
        {
            _User user = GetUser(Request);
            return new ActivityInstanceHelper(_configuration, user.UserId, user.Token).GetComments(activityInstanceRefId); ;
        }
        #endregion

        #region Action 
        [Route("ActionInstance/Execute")]
        public void ExecuteActionInstance(Guid actionRefId, Guid activityInstanceRefId)
        {
            _User user = GetUser(Request);
            ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(_configuration, user.UserId, user.Token);
            actionInstanceHelper.Execute(actionRefId, activityInstanceRefId);
        }

        [Route("ActionInstance/Execute/Anonymous")]
        public void ExecuteAnonymousActionInstance(Guid actionRefId, Guid activityInstanceRefId)
        {
            ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(_configuration, Guid.Empty);
            actionInstanceHelper.Execute(actionRefId, activityInstanceRefId);
        }

        [Route("ActionInstance/Execute/ByFormInstance")]
        public void ExecuteActionInstanceFormInstance(Guid formInstanceId)
        {
            _User user = GetUser(Request);
            ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(_configuration, user.UserId, user.Token);
            actionInstanceHelper.Execute(formInstanceId);
        }

        [Route("SystemAccionInstance/Finish")]
        public void FinishSystemAction(Guid systemActionInstanceId)
        {
            _User user = GetUser(Request);
            SystemActionInstanceHelper systemActionInstanceHelper = new SystemActionInstanceHelper(_configuration, user.UserId, user.Token);
            systemActionInstanceHelper.Finish(systemActionInstanceId);
        }

        [Route("SystemAccionInstance/Finish/Anonymous")]
        public void FinishAnonymousSystemAction(Guid systemActionInstanceId)
        {
            SystemActionInstanceHelper systemActionInstanceHelper = new SystemActionInstanceHelper(_configuration, Guid.Empty);
            systemActionInstanceHelper.Finish(systemActionInstanceId);
        }

        [Route("SystemAccionInstance/Execute")]
        public void ExecuteSystemAction(Guid systemActionInstanceId)
        {
            SystemActionInstanceHelper systemActionInstanceHelper = new SystemActionInstanceHelper(_configuration, Guid.Empty);
            systemActionInstanceHelper.Execute(systemActionInstanceId);
        }
        #endregion

        #region Tools   
        private _User GetUser(HttpRequest request)
        {
            _User user = new _User();
            Payload payload = new Payload();
            if (!request.Headers.ContainsKey("Authorization"))
                throw new Exception("The access is invalid");

            var authHeader = request.Headers["Authorization"][0];
            if (authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length);

                try
                {
                    payload = JWToken.TokenDecode(token);
                    user = new _User() { UserId = Guid.Parse(payload.UserId), Token = authHeader };
                }
                catch (Exception e)
                {
                    throw new Exception("The access is invalid");
                }
            }

            if (user.UserId == Guid.Empty)
                throw new Exception("The access is invalid");

            return user;
        }
        #endregion

    }
}