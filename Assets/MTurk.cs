using System;
using System.Collections.Generic;
using UnityEngine;
using Amazon.MTurk;
using Amazon.MTurk.Internal;
using Amazon.MTurk.Model;
public class MTurk : MonoBehaviour
{
    private AmazonMTurkClient mturkClient;
    private string hitID;
    void Start()
    {
            string awsAccessKeyId = "AKIAIT2HJ3IIGKTQRHYQ";
            string awsSecretAccessKey = "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"; 
            string SANDBOX_URL = "https://mturk-requester-sandbox.us-east-1.amazonaws.com";
            string PROD_URL = "https://requester.us-east-1.amazonaws.com"; 
            AmazonMTurkConfig config = new AmazonMTurkConfig();
            config.ServiceURL = SANDBOX_URL; 
            mturkClient = new AmazonMTurkClient(awsAccessKeyId,awsSecretAccessKey,config);
            GetAccountBalanceRequest request = new GetAccountBalanceRequest();
            GetAccountBalanceResponse balance = mturkClient.GetAccountBalance(request);


        UnityEngine.Debug.Log("Your account balance is " + balance.AvailableBalance);            // Keep the console window open in debug mode.

        // Read the XML question file into a string
        string questionXML = System.IO.File.ReadAllText(@"C:\Users\Ansh\Desktop\JacobsLab\externalquestion.xml");
          //string questionXML = System.IO.File.ReadAllText(@"C:\Users\Ansh\Downloads\mturk-code-samples-master\mturk-code-samples-master\Python\my_question.xml");
        //   UnityEngine.Debug.Log(questionXML);

        /*
        QualificationRequirement qualApprovalRate = new QualificationRequirement();
        List<QualificationRequirement> qualList = new List<QualificationRequirement>();
      
        
        qualApprovalRate.QualificationTypeId = "000000000000000000L0";

        qualApprovalRate.Comparator = Comparator.EqualTo;
        List<int> qualVals = new List<int>();
        qualVals.Add(95);
        qualApprovalRate.IntegerValues = qualVals;

        qualList.Add(qualApprovalRate);
        */
        //   qualApprovalRate.IntegerValue = 95;

        //        qualApprovalRate.IntegerValueSpecified = true;



        // Create the HIT
        CreateHITRequest hitRequest = new CreateHITRequest();
        // hitRequest.Title = "U01 Task WIP";
        //hitRequest.Description = "Work in progress Spatial and Verbal Memory Task";

        hitRequest.Title = "Basic task";
        UnityEngine.Debug.Log("HIT title " + hitRequest.Title);
        hitRequest.Description = "Testing mTurk API";
        hitRequest.Reward = "0.11";
        hitRequest.AssignmentDurationInSeconds = 60 * 60; //1 hr
    //    hitRequest.QualificationRequirements = qualList;
        hitRequest.LifetimeInSeconds = 60 * 60 * 24; // 1 day
        hitRequest.Question = questionXML;
        //ApproveAssignmentRequest approveRequest = new ApproveAssignmentRequest();
     //   approveRequest.AssignmentId =
       // mturkClient.ApproveAssignment(approveRequest);

        CreateHITResponse hit = mturkClient.CreateHIT(hitRequest);            // Show a link to the HIT 
        hitID = hit.HIT.HITId;
        UnityEngine.Debug.Log("https://workersandbox.mturk.com/projects/" + hit.HIT.HITTypeId + "/tasks");

    }

    public void GetAssignmentFunc()
    {


        ListAssignmentsForHITRequest req = new ListAssignmentsForHITRequest();
        UnityEngine.Debug.Log("HIT ID " + hitID);
        req.HITId = hitID;
        List<string> assStatus = new List<string>();
        assStatus.Add("Submitted");
        assStatus.Add("Approved");
        req.AssignmentStatuses = assStatus;
        req.MaxResults = 10;

        ListAssignmentsForHITResponse hitRes = mturkClient.ListAssignmentsForHIT(req);

        List<Assignment> assignList = hitRes.Assignments;

        foreach(var assign in assignList)
        {
            string assignmentID = assign.AssignmentId;
            GetAssignmentRequest getAssignReq = new GetAssignmentRequest();
            getAssignReq.AssignmentId = assignmentID;

            UnityEngine.Debug.Log("approving assignment ID " + assignmentID);

            GetAssignmentResponse getAssignResponse = mturkClient.GetAssignment(getAssignReq);
            ApproveAssignmentRequest approveAssignmentRequest = new ApproveAssignmentRequest();
            approveAssignmentRequest.AssignmentId = assignmentID;
            approveAssignmentRequest.OverrideRejection = false;
            approveAssignmentRequest.RequesterFeedback = "Good";
            ApproveAssignmentResponse approveAssignmentResponse = mturkClient.ApproveAssignment(approveAssignmentRequest);
            UnityEngine.Debug.Log("APPROVED");
        }
    }

    public void Response()
    {
        GetHITRequest gHit = new GetHITRequest();
        gHit.HITId = hitID;
        GetHITResponse hitResponse = mturkClient.GetHIT(gHit);

        UnityEngine.Debug.Log("HIT Title: " + hitResponse.HIT.Title);
        UnityEngine.Debug.Log("HIT Status: " + hitResponse.HIT.HITStatus);

        UnityEngine.Debug.Log("assignments available: " + hitResponse.HIT.NumberOfAssignmentsAvailable.ToString());
        UnityEngine.Debug.Log("assignments pending: " + hitResponse.HIT.NumberOfAssignmentsPending.ToString());
        UnityEngine.Debug.Log("assignments completed: " + hitResponse.HIT.NumberOfAssignmentsCompleted.ToString());

        /*
        ListAssignmentsForHITRequest req = new ListAssignmentsForHITRequest();
        UnityEngine.Debug.Log("HIT ID " + hitID);
        req.HITId = hitID;
        List<string> assStatus = new List<string>();
        assStatus.Add("Submitted");
        req.AssignmentStatuses = assStatus;
        req.MaxResults = 10;

        mturkClient.ListAssignmentsForHIT(req);
        GetAssignmentRequest getAssignReq = new GetAssignmentRequest();
        GetAssignmentResponse getAssignResponse = mturkClient.GetAssignment(getAssignReq);
        */
        //    UnityEngine.Debug.Log(getAssignResponse.Assignment.ToString());

    }
}
