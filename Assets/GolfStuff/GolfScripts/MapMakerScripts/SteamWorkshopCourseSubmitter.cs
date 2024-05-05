using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System;

public class SteamWorkshopCourseSubmitter : MonoBehaviour
{
    private SteamAPICall_t createItemCall;
    private CallResult<CreateItemResult_t> createCallRes;
    private UGCUpdateHandle_t updateHandle;
    private string _path;
    private CourseData _courseToUpload;
    private int _numberOfCompletedHoles = 0;

    private CallResult<SubmitItemUpdateResult_t> ItemUpdateResult;

    [SerializeField] MapMakerUIManager _mapMakerUIManager;

    // Start is called before the first frame update
    void Start()
    {
        if (!_mapMakerUIManager)
            _mapMakerUIManager = this.GetComponent<MapMakerUIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable()
    {
        ItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(OnItemUpdateResult);
    }

    public void UploadCourseToSteamWorkshop(CourseData courseToUpload)
    {
        Debug.Log("UploadCourseToSteamWorkshop: ");
        if (!SteamManager.Initialized)
        {
            Debug.Log("UploadCourseToSteamWorkshop: Steam manager not initialized");
            return;
        }
            
        if (courseToUpload == null)
        {
            Debug.Log("UploadCourseToSteamWorkshop: courseToUpload is null");
            return;
        }
        if (string.IsNullOrEmpty(courseToUpload.RelativeFilePath))
        {
            Debug.Log("UploadCourseToSteamWorkshop: course does not have a RelativeFilePath set");
            return;
        }

        _courseToUpload = courseToUpload;

        if (_courseToUpload.HolesInCourse.Count <= 0)
        {
            Debug.Log("UploadCourseToSteamWorkshop: HolesInCourse is 0");
            return;
        }

        if (!_courseToUpload.HolesInCourse.Any(x => x.IsHoleCompleted))
        {
            Debug.Log("UploadCourseToSteamWorkshop: no holes in the course are complete");
            return;
        }

        _path = Application.persistentDataPath + "/" + courseToUpload.RelativeFilePath;

        if (!File.Exists(_path))
        {
            Debug.Log("UploadCourseToSteamWorkshop: file could not be found at path: " + _path);
            return;
        }

        SetNumberOfCompletedHoles(courseToUpload);
        if (_numberOfCompletedHoles <= 0)
        {
            Debug.Log("UploadCourseToSteamWorkshop: _numberOfCompletedHoles is 0");
            return;
        }
        if (courseToUpload.WorkshopPublishedItemID != 0)
        {
            //SteamAPICall_t handle = SteamUGC.StartItemUpdate(new AppId_t(2071510), new PublishedFileId_t(courseToUpload.WorkshopPublishedItemID));
            UpdateExistingWorkshopFile(courseToUpload.WorkshopPublishedItemID);
        }
        else
        {
            createCallRes = CallResult<CreateItemResult_t>.Create(OnCreateItem);
            SteamAPICall_t handle = SteamUGC.CreateItem(new AppId_t(2071510), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
            createCallRes.Set(handle);
        }
        
    }
    void SetNumberOfCompletedHoles(CourseData course)
    {
        if (course == null)
            return;
        _numberOfCompletedHoles = 0;
        if (course.HolesInCourse.Count > 0)
        {
            foreach (HoleData hole in course.HolesInCourse)
            {
                if (hole.IsHoleCompleted)
                    _numberOfCompletedHoles++;
            }
        }
    }
    void OnCreateItem(CreateItemResult_t pCallback, bool bIOFailure)
    {
        if (!SteamManager.Initialized)
            return;
        Debug.Log("SteamWorkshopCourseSubmitter: OnCreateItem: ");
        if (!pCallback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
        {
            Debug.Log("SteamWorkshopCourseSubmitter: OnCreateItem: pCallback.m_bUserNeedsToAcceptWorkshopLegalAgreement is true?");
            //Once a workshop item has been created and a PublishedFileId_t value has been returned, the content of the workshop item can be populated and uploaded to the Steam Workshop.
            //An item update begins with a call to SteamUGC.StartItemUpdate

            updateHandle = SteamUGC.StartItemUpdate(new AppId_t(2071510), pCallback.m_nPublishedFileId); //may need to do a create and onFunction to set the handle of UGCUpdateHandle_t
            SteamUGC.SetItemTitle(updateHandle, _courseToUpload.CourseName);
            SteamUGC.SetItemDescription(updateHandle, "Number of complete holes to play: " + _numberOfCompletedHoles.ToString());
            SteamUGC.SetItemContent(updateHandle, _path);

            // item preview image stuff I haven't implemented yet...
            //string newImagePath = "";
            //if (m_textPath != null)
            //{
            //    newImagePath = m_textPath.Replace("\\", "/");
            //}
            //if (File.Exists(newImagePath))
            //{
            //    SteamUGC.SetItemPreview(updateHandle, newImagePath);
            //    //print("Setting " + newImagePath + " as preview image");
            //}

            SteamUGC.SetItemVisibility(updateHandle, 0); //k_ERemoteStoragePublishedFileVisibilityPublic = 0, so it should be set to public with this line

            //Once the update calls have been completed, calling ISteamUGC::SubmitItemUpdate will initiate the upload process to the Steam Workshop.
            SteamUGC.SubmitItemUpdate(updateHandle, "New workshop item");
            //SteamFriends.ActivateGameOverlayToWebPage("steam://url/CommunityFilePage/" + pCallback.m_nPublishedFileId);

            _mapMakerUIManager.SetWorkshopIDAfterUpload((ulong)pCallback.m_nPublishedFileId);

        }
        else
        {
            Debug.Log("SteamWorkshopCourseSubmitter: OnCreateItem: pCallback.m_bUserNeedsToAcceptWorkshopLegalAgreement is false?");
            RedirectToLegal();
        }
    }
    void UpdateExistingWorkshopFile(ulong publishedFileID)
    {
        Debug.Log("UpdateExistingWorkshopFile: " + publishedFileID.ToString());
        UGCUpdateHandle_t m_UGCUpdateHandle = SteamUGC.StartItemUpdate(new AppId_t(2071510), new PublishedFileId_t(publishedFileID));
        SteamUGC.SetItemDescription(m_UGCUpdateHandle, "Number of complete holes to play: " + _numberOfCompletedHoles.ToString());
        bool ret = SteamUGC.SetItemContent(m_UGCUpdateHandle, _path);
        if (ret)
        {
            SteamAPICall_t handle = SteamUGC.SubmitItemUpdate(m_UGCUpdateHandle, "Update Golf Course on: " + DateTime.Today.ToString());
            ItemUpdateResult.Set(handle);
        }
    }
    private void OnItemUpdateResult(SubmitItemUpdateResult_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log("OnItemUpdateResult: result ok");
        }
        else
        {
            Debug.Log("OnItemUpdateResult: result failure?");
        }
    }
    public void RedirectToLegal()
    {
        Debug.Log("RedirectToLegal: ");
        SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/workshoplegalagreement");
    }
}
