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
        public IConfiguration Configuration { get; }

        public ExecutionController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
               
        #region ProcedureInstance
        [Route("ProcedureInstance")]
        public ProcedureInstance GetProcedureInstance(Guid procedureInstanceRefId)
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(Configuration, user.Id, user.Token).Get(procedureInstanceRefId);
        }

        [Route("ProcedureInstance/Create")]
        public ProcedureInstanceCreated CreateProcedureInstance(Guid procedureRefId)
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(Configuration, user.Id, user.Token).Create(procedureRefId);
        }

        [Route("ProcedureInstance/Create/Anonymous")]
        public ProcedureInstanceCreated CreateAnonymousProcedureInstance(Guid procedureRefId)
        {
            return new ProcedureInstanceHelper(Configuration, Guid.Empty).Create(procedureRefId);
        }

        [Route("ProcedureInstance/GetInProcess")]
        public List<ProcedureInstanceSummary> GetProcedureInstanceInProcess()
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(Configuration, user.Id, user.Token).GetInProcess();
        }

        [Route("ProcedureInstance/GetInProcessCount")]
        public int GetProcedureInstanceInProcessCount()
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(Configuration, user.Id, user.Token).GetInProcessCount();
        }

        [Route("ProcedureInstance/GetResolved")]
        public List<ProcedureInstanceSummary> GetProcedureInstanceResolved()
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(Configuration, user.Id, user.Token).GetResolved();
        }

        [Route("ProcedureInstance/GetLog")]
        public List<ActivityInstanceSummary> GetProcedureInstanceLog(Guid procedureInstanceRefId)
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(Configuration, user.Id, user.Token).GetLog(procedureInstanceRefId);
        }

        [Route("ProcedureInstance/GetComments")]
        public List<Comment> GetProcedureInstanceComments(Guid procedureInstanceRefId)
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(Configuration, user.Id, user.Token).GetComments(procedureInstanceRefId);
        }

        [Route("ProcedureInstance/SetComment")]
        public string SetProcedureInstanceComment(Guid elementInstanceRefId, string comment)
        {
            _User user = GetUser(Request);
            ProcedureInstanceHelper procedureInstanceHelper = new ProcedureInstanceHelper(Configuration, user.Id, user.Token);
            procedureInstanceHelper.SetComment(elementInstanceRefId, comment);
            return comment;
        }
        #endregion

        #region FlowInstance
        [Route("FlowInstance")]
        public FlowInstance GetFlowInstance(Guid flowInstanceRefId)
        {
            _User user = GetUser(Request);
            return new FlowInstanceHelper(Configuration, user.Id, user.Token).Get(flowInstanceRefId);
        }
        
        [Route("FlowInstance/SetContentProperty")]
        public dynamic SetContentProperty(Guid flowInstanceRefId, string propertyName, string value)
        {
            _User user = GetUser(Request);
            return new FlowInstanceHelper(Configuration, user.Id, user.Token).SetContentProperty(flowInstanceRefId, propertyName, value);
        }

        [Route("FlowInstance/SetContentProperty/ByFormInstance")]
        public dynamic SetContentPropertyFormInstance(Guid formInstanceId, string propertyName, string value)
        {
            _User user = GetUser(Request);
            return new FlowInstanceHelper(Configuration, user.Id, user.Token).SetContentPropertyFormInstance(formInstanceId, propertyName, value);
        }

        [HttpPost]
        [Route("FlowInstance/SetUserGroup")]
        public void SetUserGroup(Guid flowInstanceRefId, [FromBody] UserGroup[] users)
        {
            _User user = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(Configuration, user.Id, user.Token);
            flowInstanceHelper.SetUserGroup(flowInstanceRefId, users);
        }

        [HttpPost]
        [Route("FlowInstance/SetUserGroup/ByFormInstance")]
        public void SetUserGroupFormInstance(Guid formInstanceId, [FromBody] UserGroup[] users)
        {
            _User user = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(Configuration, user.Id, user.Token);
            flowInstanceHelper.SetUserGroupFormInstance(formInstanceId, users);
        }

        [Route("FlowInstance/SetState")]
        public void SetState(Guid elementInstanceRefId, string key, string state)
        {
            _User user = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(Configuration, user.Id, user.Token);
            flowInstanceHelper.SetState(elementInstanceRefId, key, state);
        }

        [Route("FlowInstance/SetState/ByFormInstance")]
        public void SetStateFormInstance(Guid formInstanceId, string key, string state)
        {
            _User user = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(Configuration, user.Id, user.Token);
            flowInstanceHelper.SetStateFormInstance(formInstanceId, key, state);
        }
        #endregion

        #region Message  
        [Route("Message/GetOpen")]
        public OpenedMessage GetMessageOpen(Guid messageId)
        {
            _User user = GetUser(Request);
            return new MessageHelper(Configuration, user.Id, user.Token).GetOpen(messageId);
        }

        [Route("Message/GetReceived")]
        public List<Message> GetMessagesReceived()
        {
            _User user = GetUser(Request);
            return new MessageHelper(Configuration, user.Id, user.Token).GetReceived();
        }

        [Route("Message/GetReceivedCount")]
        public int GetMessagesReceivedCount()
        {
            _User user = GetUser(Request);
            return new MessageHelper(Configuration, user.Id, user.Token).GetReceivedCount();
        }

        [Route("Message/GetDrafts")]
        public List<Message> GetMessagesDrafts()
        {
            _User user = GetUser(Request);
            return new MessageHelper(Configuration, user.Id, user.Token).GetDrafts();
        }
        #endregion

        #region Activity 
        [Route("ActivityInstance")]
        public ActivityInstance GetActivityInstance(Guid elementInstanceRefId)
        {
            _User user = GetUser(Request);
            return new ActivityInstanceHelper(Configuration, user.Id, user.Token).Get(elementInstanceRefId);
        }

        [Route("ActivityInstance/Anonymous")]
        public ActivityInstance GetAnonymousActivityInstance(Guid elementInstanceRefId)
        {
            return new ActivityInstanceHelper(Configuration, Guid.Empty).Get(elementInstanceRefId);
        }

        [Route("ActivityInstance/IsAnonymous")]
        public string GetIsAnonymousActivityInstance(Guid elementInstanceRefId)
        {
            return new ActivityInstanceHelper(Configuration, Guid.Empty).IsAnonymous(elementInstanceRefId);
        }

        [Route("ActivityInstance/GetComments")]
        public List<Comment> GetActivityInstanceComments(Guid elementInstanceRefId)
        {
            _User user = GetUser(Request);
            return new ActivityInstanceHelper(Configuration, user.Id, user.Token).GetComments(elementInstanceRefId);
        }

        [Route("ActivityInstance/SetComment")]
        public string SetActivityInstanceComment(Guid elementInstanceRefId, string comment)
        {
            _User user = GetUser(Request);
            ActivityInstanceHelper activityInstanceHelper = new ActivityInstanceHelper(Configuration, user.Id, user.Token);
            activityInstanceHelper.SetComment(elementInstanceRefId, comment);
            return comment;
        }
        #endregion

        #region Action 
        [Route("ActionInstance/Execute")]
        public void ExecuteActionInstance(Guid actionRefId, Guid elementInstanceRefId)
        {
            _User user = GetUser(Request);
            ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(Configuration, user.Id, user.Token);
            actionInstanceHelper.Execute(actionRefId, elementInstanceRefId);
        }

        [Route("ActionInstance/Execute/Anonymous")]
        public void ExecuteAnonymousActionInstance(Guid actionRefId, Guid elementInstanceRefId)
        {
            ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(Configuration, Guid.Empty);
            actionInstanceHelper.Execute(actionRefId, elementInstanceRefId);
        }
        #endregion

        #region SystemAction
        [Route("SystemAccionInstance/Finish")]
        public void FinishSystemAction(Guid systemActionInstanceId)
        {
            _User user = GetUser(Request);
            SystemActionInstanceHelper systemActionInstanceHelper = new SystemActionInstanceHelper(Configuration, user.Id, user.Token);
            systemActionInstanceHelper.Finish(systemActionInstanceId);
        }

        [Route("SystemAccionInstance/Finish/Anonymous")]
        public void FinishAnonymousSystemAction(Guid systemActionInstanceId)
        {
            SystemActionInstanceHelper systemActionInstanceHelper = new SystemActionInstanceHelper(Configuration, Guid.Empty);
            systemActionInstanceHelper.Finish(systemActionInstanceId);
        }

        [Route("SystemAccionInstance/Execute")]
        public void ExecuteSystemAction(Guid systemActionInstanceId)
        {
            //_User user = GetUser(Request);
            SystemActionInstanceHelper systemActionInstanceHelper = new SystemActionInstanceHelper(Configuration, Guid.Empty);
            systemActionInstanceHelper.Execute(systemActionInstanceId);
        }
        #endregion

        #region User
        [Route("User")]
        public User GetUser(Guid ownerId)
        {
            _User user = GetUser(Request);
            return new UserHelper(Configuration, user.Id, user.Token).Get(ownerId);
        }

        [Route("User/GetOwnerRoles")]
        public List<string> GetUserOwnerRoles(Guid ownerId)
        {
            _User user = GetUser(Request);            
            return new UserHelper(Configuration, user.Id, user.Token).GetOwnerRoles(ownerId); ;
        }

        [Route("User/Create")]
        [HttpPost]
        public void GetCreateUser(Guid userId, Guid ownerId, string userName, string givenName, string rfc, [FromForm] string content, string familyName = "", string email = "")
        {
            _User user = GetUser(Request);
            UserHelper userHelper = new UserHelper(Configuration, user.Id, user.Token);
            userHelper.Create(userId, ownerId, userName, givenName, familyName, email, rfc, content);
        }

        #endregion

        #region Tools   
        private _User GetUser(HttpRequest request)
        {
            _User user = new _User();

            if (!request.Headers.ContainsKey("Authorization"))
                throw new Exception("The access is invalid");

            var authHeader = request.Headers["Authorization"][0];
            if (authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length);

                try
                {
                    Payload payload = JWToken.TokenDecode(token);
                    user = new _User() { Id = Guid.Parse(payload.UserId), Token = authHeader };
                }
                catch (Exception)
                {
                    throw new Exception("The access is invalid");
                }
            }

            if (user.Id == Guid.Empty)
                throw new Exception("The access is invalid");

            return user;
        }
        #endregion

    }
}