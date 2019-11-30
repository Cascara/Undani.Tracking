using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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

        [Route("ProcedureInstance/GetToTransfer")]
        public List<ProcedureInstanceSummary> GetProcedureInstanceToTransfer()
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(Configuration, user.Id, user.Token).GetToTransfer();
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

        [Route("ProcedureInstance/SetState")]
        public bool SetProcedureInstanceState(Guid procedureInstanceRefId, string key, string state)
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(Configuration, user.Id, user.Token).SetState(procedureInstanceRefId, key, state);
        }

        [Route("ProcedureInstance/GetState")]
        public dynamic GetProcedureInstanceState(Guid procedureInstanceRefId)
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(Configuration, user.Id, user.Token).GetState(procedureInstanceRefId);
        }

        [HttpPost]
        [Route("ProcedureInstance/SetDocumentSigned")]
        public void SetProcedureInstanceDocumentSigned(Guid procedureInstanceRefId, string key, [FromBody] DocumentSigned[] documentSigneds)
        {
            _User user = GetUser(Request);

            ProcedureInstanceHelper procedureInstanceHelper = new ProcedureInstanceHelper(Configuration, user.Id, user.Token);
            procedureInstanceHelper.SetDocumentsSigned(procedureInstanceRefId, key, documentSigneds);
        }

        [HttpPost]
        [Route("ProcedureInstance/SetContentProperty")]
        public dynamic SetProcedureInstanceContentProperty(Guid procedureInstanceRefId, [FromForm] string value, string type = "", string propertyName = ".")
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(Configuration, user.Id, user.Token).SetContentProperty(procedureInstanceRefId, propertyName, value, type);
        }

        [Route("ProcedureInstance/Transfer")]
        public bool ProcedureInstanceTransfer(Guid elementInstanceRefId, Guid destinyUserId, string key)
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(Configuration, user.Id, user.Token).Transfer(elementInstanceRefId, destinyUserId, key);
        }

        [Route("ProcedureInstance/TransferAll")]
        public bool ProcedureInstanceTransferAll(Guid sourceUserId, Guid destinyUserId)
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(Configuration, user.Id, user.Token).TransferAll(sourceUserId, destinyUserId);
        }

        [Route("ProcedureInstance/GetUserSelected")]
        public List<UserSelected> GetProcedureInstanceTUserSelected(Guid procedureInstanceRefId)
        {
            _User user = GetUser(Request);
            return new ProcedureInstanceHelper(Configuration, user.Id, user.Token).GetUserSelected(procedureInstanceRefId);
        }
        #endregion

        #region FlowInstance
        [Route("FlowInstance")]
        public FlowInstance GetFlowInstance(Guid flowInstanceRefId)
        {
            _User user = GetUser(Request);
            return new FlowInstanceHelper(Configuration, user.Id, user.Token).Get(flowInstanceRefId);
        }

        [HttpPost]
        [Route("FlowInstance/SetContentProperty")]
        public dynamic SetFlowInstanceContentProperty(Guid flowInstanceRefId, [FromForm] string value, string type = "", string propertyName = ".")
        {
            _User user = GetUser(Request);
            return new FlowInstanceHelper(Configuration, user.Id, user.Token).SetContentProperty(flowInstanceRefId, propertyName, value, type);
        }

        [HttpPost]
        [Route("FlowInstance/SetUserGroup")]
        public void SetUserGroup(Guid flowInstanceRefId, [FromBody] UserGroup[] users)
        {
            _User user = GetUser(Request);
            FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(Configuration, user.Id, user.Token);
            flowInstanceHelper.SetUserGroup(flowInstanceRefId, users);
        }

        [Route("FlowInstance/SetState")]
        public bool SetFlowInstanceState(Guid flowInstanceRefId, string key, string state)
        {
            _User user = GetUser(Request);
            return new FlowInstanceHelper(Configuration, user.Id, user.Token).SetState(flowInstanceRefId, key, state);
        }

        [Route("FlowInstance/GetState")]
        public dynamic GetFlowInstanceState(Guid flowInstanceRefId)
        {
            _User user = GetUser(Request);
            return new FlowInstanceHelper(Configuration, user.Id, user.Token).GetState(flowInstanceRefId);
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

        #region ActivityInstance 
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

        [Route("ActivityInstance/GetSignature")]
        public ActivityInstanceSignature GetSignature(Guid elementInstanceRefId)
        {
            _User user = GetUser(Request);
            return new ActivityInstanceHelper(Configuration, user.Id, user.Token).GetSignature(elementInstanceRefId);
        }

        [Route("ActivityInstance/GetSignatureTemplate")]
        public ActivityInstanceSignature GetSignatureTemplate(Guid elementInstanceRefId, string template)
        {
            _User user = GetUser(Request);
            return new ActivityInstanceHelper(Configuration, user.Id, user.Token).GetSignatureTemplate(elementInstanceRefId, template);
        }

        [HttpPost]
        [Route("ActivityInstance/SetDocumentSigned")]
        public void SetActivityInstanceDocumentSigned(Guid elementInstanceRefId, string key, [FromBody] DocumentSigned[] documentSigneds)
        {
            _User user = GetUser(Request);

            ActivityInstanceHelper activityInstanceHelper = new ActivityInstanceHelper(Configuration, user.Id, user.Token);
            activityInstanceHelper.SetDocumentsSigned(elementInstanceRefId, key, documentSigneds);
        }
        #endregion

        #region ActionInstance 
        [Route("ActionInstance/Execute")]
        public bool ExecuteActionInstance(Guid actionRefId, Guid elementInstanceRefId)
        {
            _User user = GetUser(Request);
            ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(Configuration, user.Id, user.Token);
            return actionInstanceHelper.Execute(actionRefId, elementInstanceRefId);
        }

        [Route("ActionInstance/Execute/Anonymous")]
        public bool ExecuteAnonymousActionInstance(Guid actionRefId, Guid elementInstanceRefId)
        {
            ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(Configuration, Guid.Empty);
            return actionInstanceHelper.Execute(actionRefId, elementInstanceRefId);
        }
        #endregion

        #region SystemAccionInstance
        [HttpPost]
        [Route("SystemAccionInstance/Finish")]
        public string FinishSystemAction(Guid systemActionInstanceId, [FromForm] string procedureInstanceContent = "{}", [FromForm] string flowInstanceContent = "{}")
        {
            try
            {
                SystemActionInstanceHelper systemActionInstanceHelper = new SystemActionInstanceHelper(Configuration, Guid.Empty);
                systemActionInstanceHelper.Finish(systemActionInstanceId, procedureInstanceContent, flowInstanceContent);

                return "Success";
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(ex);
            }
        }

        [HttpPost]
        [Route("SystemAccionInstance/Success")]
        public string SuccessSystemAction([FromBody] SystemActionInstanceResponse systemActionInstanceResponse)
        {
            try
            {
                SystemActionInstanceHelper systemActionInstanceHelper = new SystemActionInstanceHelper(Configuration, Guid.Empty);
                systemActionInstanceHelper.Finish(systemActionInstanceResponse.SystemActionInstanceId, systemActionInstanceResponse.ProcedureInstanceContent, systemActionInstanceResponse.FlowInstanceContent);

                return "Success";
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(ex);
            }
        }

        [Route("SystemAccionInstance/Execute")]
        public Exception ExecuteSystemAction(Guid systemActionInstanceId)
        {
            //try
            //{
                SystemActionInstanceHelper systemActionInstanceHelper = new SystemActionInstanceHelper(Configuration, Guid.Empty);
                systemActionInstanceHelper.Execute(systemActionInstanceId);

                return null;
            //}
            //catch (Exception ex)
            //{
            //    return ex;
            //}
        }
        #endregion

        #region User
        [Route("User")]
        public User GetUser(Guid ownerId)
        {
            _User user = GetUser(Request);
            return new UserHelper(Configuration, user.Id, user.Token).Get(ownerId);
        }

        [Route("User/GetAll")]
        public List<UserSummary> GetAllUsers(string role) { 
            _User user = GetUser(Request);
            return new UserHelper(Configuration, user.Id, user.Token).Get(role);
        }

        [Route("User/GetOwnerRoles")]
        public List<string> GetUserOwnerRoles(Guid ownerId)
        {
            _User user = GetUser(Request);            
            return new UserHelper(Configuration, user.Id, user.Token).GetOwnerRoles(ownerId);
        }

        [Route("User/Create")]
        [HttpPost]
        public void CreateUser(Guid userId, Guid ownerId, string reference, string roles, string userName, string givenName, [FromForm] string content, string familyName = "", string email = "")
        {
            _User user = GetUser(Request);
            UserHelper userHelper = new UserHelper(Configuration, user.Id, user.Token);
            userHelper.Create(userId, ownerId, reference, roles, userName, givenName, familyName, email, content);
        }

        [Route("User/SetContent")]
        [HttpPost]
        public void SetUserContent(Guid userId, [FromForm] string content)
        {
            _User user = GetUser(Request);
            UserHelper userHelper = new UserHelper(Configuration, user.Id, user.Token);
            userHelper.SetContent(userId, content);
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
                catch (Exception ex)
                {
                    throw new Exception("The access is invalid");
                }
            }

            if (user.Id == Guid.Empty)
                throw new Exception("The access is invalid");

            return user;
        }

        [Route("Daily")]
        public bool Daily()
        {
            MessageHelper messageHelper = new MessageHelper(Configuration, Guid.Empty);
            messageHelper.SetStatus();

            SystemActionInstanceHelper systemActionInstanceHelper = new SystemActionInstanceHelper(Configuration, Guid.Empty);
            systemActionInstanceHelper.ExecuteDaily();

            return true;
        }
        #endregion

        #region Temporary



        #endregion
    }
}