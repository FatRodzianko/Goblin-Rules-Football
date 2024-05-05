using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class SteamWorkshopCourseSubmitter : MonoBehaviour
{
    private SteamAPICall_t createItemCall;
    private CallResult<CreateItemResult_t> createCallRes;
    private UGCUpdateHandle_t updateHandle;
    private string _path;
    private CourseData _courseToUpload;
    private int _numberOfCompletedHoles = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UploadCourseToSteamWorkshop(CourseData courseToUpload)
    {
        if (!SteamManager.Initialized)
            return;
        if (courseToUpload == null)
            return;
        if (string.IsNullOrEmpty(courseToUpload.RelativeFilePath))
            return;

        _courseToUpload = courseToUpload;

        if (_courseToUpload.HolesInCourse.Count <= 0)
            return;

        if (!_courseToUpload.HolesInCourse.Any(x => x.IsHoleCompleted))
            return;

        _path = Application.persistentDataPath + "/" + courseToUpload.RelativeFilePath;

        if (!File.Exists(_path))
            return;

        createCallRes = CallResult<CreateItemResult_t>.Create(OnCreateItem);
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

        if (!pCallback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
        {
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
            SteamFriends.ActivateGameOverlayToWebPage("steam://url/CommunityFilePage/" + pCallback.m_nPublishedFileId);

        }
        else
        {
            redirectToLegal();
        }
    }
    public void redirectToLegal()
    {
        SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/workshoplegalagreement");
    }
}
