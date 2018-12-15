using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Undani.Tracking.Execution.Core;
using Undani.Tracking.Execution.Core.Infra;

namespace Undani.Tracking.Execution.API.Controllers
{
    [Produces("application/json")]
    [Route("Execution")]
    public class ExecutionController : Controller
    {
        #region ProcedureInstance
        [Route("ProcedureInstance/")]
        public ProcedureInstance GetProcedureInstance(Guid uid, Guid procedureRefId)
        {

        }

        [Route("ProcedureInstance/Create")]
        public ProcedureInstance CreateProcedureInstance (Guid uid, Guid procedureRefId, string content, Guid environmentId, Guid? procedureInstanceSourceRefId = null)
        {

        }
        #endregion

        #region FlowInstance
        [Route("FlowInstance")]
        public FlowInstance GetFlowInstance(Guid uid, Guid flowInstanceRefId)
        {
            FlowInstance result = FlowInstanceHelper.Get(uid, flowInstanceRefId);
            return result;
        }
        
        [Route("FlowInstance/Create")]
        public FlowInstanceSummary CreateFlowInstance(Guid uid, Guid flowRefId, Guid environmentId, string content, Guid? activityInstanceParentRefId = null, string version = "")
        {
            return FlowInstanceHelper.Create(uid, flowRefId, environmentId, content, activityInstanceParentRefId, version);
        }

        [Route("FlowInstance/Create/ByFormInstance")]
        public FlowInstanceSummary CreateFlowInstanceFormInstance(Guid uid, Guid flowRefId, Guid environmentId, string content, string formInstanceKey, Guid formInstanceId, Guid? activityInstanceParentRefId = null, string version = "")
        {
            return FlowInstanceHelper.CreateByFormInstanceId(uid, flowRefId, environmentId, content, formInstanceKey, formInstanceId, activityInstanceParentRefId, version);
        }

        [Route("FlowInstance/SetContentProperty")] 
        public dynamic SetContentProperty(Guid uid, Guid flowInstanceRefId, string propertyName, string value)
        {
            return FlowInstanceHelper.SetContentProperty(uid, flowInstanceRefId, propertyName, value);
        }

        [Route("FlowInstance/SetContentProperty/ByFormInstance")]
        public dynamic SetContentPropertyFormInstance(Guid uid, Guid formInstanceId, string propertyName, string value)
        {
            return FlowInstanceHelper.SetContentPropertyFormInstance(uid, formInstanceId, propertyName, value);
        }

        [HttpPost]
        [Route("FlowInstance/SetUserGroup")]
        public void SetUserGroup(Guid uid, Guid flowInstanceRefId, [FromBody] UserGroup[] users)
        {
            FlowInstanceHelper.SetUserGroup(uid, flowInstanceRefId, users);
        }

        [HttpPost]
        [Route("FlowInstance/SetUserGroup/ByFormInstance")]
        public void SetUserGroupFormInstance(Guid uid, Guid formInstanceId, [FromBody] UserGroup[] users)
        {
            FlowInstanceHelper.SetUserGroupFormInstance(uid, formInstanceId, users);
        }

        [Route("FlowInstance/SetState")]
        public void SetState(Guid uid, Guid activityInstanceRefId, string key, string state)
        {
            FlowInstanceHelper.SetState(uid, activityInstanceRefId, key, state);
        }

        [Route("FlowInstance/SetState/ByFormInstance")]
        public void SetStateFormInstance(Guid uid, Guid formInstanceId, string key, string state)
        {
            FlowInstanceHelper.SetStateFormInstance(uid, formInstanceId, key, state);
        }

        [Route("FlowInstance/GetLog")]
        public PagedList<FlowInstanceSummary> GetFlowLog(Guid uid, int? pageLimit = null, int? page = null)
        {
            return FlowInstanceHelper.GetLog(uid, pageLimit, page);
        }
        #endregion

        #region Message  
        [Route("Message/GetOpen")]
        public OpenedMessage GetMessageOpen(Guid uid, Guid messageId)
        {
            return MessageHelper.GetOpen(uid, messageId);
        }

        [Route("Message/GetReceived")] 
        public List<Message> GetMessagesReceived(Guid uid)
        {
            return MessageHelper.GetReceived(uid);
        }

        [Route("Message/GetDrafts")] 
        public List<Message> GetMessagesDrafts(Guid uid)
        {
            return MessageHelper.GetDrafts(uid);
        }
        #endregion

        #region Activity 
        [Route("ActivityInstance")] 
        public ActivityInstance GetActivityInstance(Guid uid, Guid activityInstanceRefId)
        {
            return ActivityInstanceHelper.Get(uid, activityInstanceRefId);
        }

        [Route("ActivityInstance/GetLog")]
        public List<ActivityInstanceSummary> GetActivityInstanceLog(Guid uid, Guid activityInstanceRefId)
        {
            List<ActivityInstanceSummary> activities = ActivityInstanceHelper.GetSummaryLog(uid, activityInstanceRefId);
            return activities;
        }

        [Route("ActivityInstance/SetComment")]
        public string SetComment(Guid uid, Guid activityInstanceRefId, string comment)
        {
            ActivityInstanceHelper.SetComment(uid, activityInstanceRefId, comment);
            return comment;
        }

        [Route("ActivityInstance/GetComments")]
        public List<Comment> GetComments(Guid uid, Guid activityInstanceRefId)
        {
            List<Comment> comments = ActivityInstanceHelper.GetComments(uid, activityInstanceRefId);
            return comments;
        }
        #endregion

        #region Action 
        [Route("ActionInstance/Execute")]
        public void ExecuteActionInstance(Guid uid, Guid actionRefId , Guid activityInstanceRefId)
        {
            ActionInstanceHelper.Execute(actionRefId, activityInstanceRefId, uid);
        }

        [Route("ActionInstance/Execute/ByFormInstance")]
        public void ExecuteActionInstanceFormInstance(Guid uid, Guid formInstanceId)
        {
            ActionInstanceHelper.Execute(uid, formInstanceId);
        }

        [Route("SystemAccionInstance/Finish")] ///TODO: Refactory
        public void FinishSystemAction(Guid id)
        {
            SystemActionInstanceHelper.Finish(id);
        }
        #endregion



    }
}