using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class SteamWorkshopCourseDownloader : MonoBehaviour
{
    bool installedStuff;
    private PublishedFileId_t publishedFileID;
    private UGCHandle_t UGCHandle;

    private CallResult<DownloadItemResult_t> DownloadItemResult;
    private CallResult<RemoteStorageGetPublishedFileDetailsResult_t> RemoteStorageGetPublishedFileDetailsResult;
    private CallResult<RemoteStorageDownloadUGCResult_t> RemoteStorageDownloadUGCResult;


    private CallResult<RemoteStorageSubscribePublishedFileResult_t> SubscribeItemResult;
    private CallResult<RemoteStorageEnumerateUserSubscribedFilesResult_t> RemoteStorageEnumerateUserSubscribedFilesResult;
    public List<PublishedFileId_t> subscribedItemList;
    //whether or not current item has finished downloading
    public bool fetchedContent;
    //the contents of the downloaded item
    private string itemContent;

    // Start is called before the first frame update
    void Start()
    {
        //SyncSubscribedToCourses();
        subscribedItemList = new List<PublishedFileId_t>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable()
    {
        DownloadItemResult = CallResult<DownloadItemResult_t>.Create(OnDownloadItemResult);
        RemoteStorageGetPublishedFileDetailsResult = CallResult<RemoteStorageGetPublishedFileDetailsResult_t>.Create(OnRemoteStorageGetPublishedFileDetailsResult);
        RemoteStorageDownloadUGCResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(OnRemoteStorageDownloadUGCResult);

        SubscribeItemResult = CallResult<RemoteStorageSubscribePublishedFileResult_t>.Create(OnRemoteStorageSubscribePublishedFileResult);
        RemoteStorageEnumerateUserSubscribedFilesResult = CallResult<RemoteStorageEnumerateUserSubscribedFilesResult_t>.Create(OnRemoteStorageEnumerateUserSubscribedFilesResult);
    }

    public void SyncSubscribedToCourses()
    {
        if (!SteamManager.Initialized)
            return;
        Debug.Log("SyncSubscribedToCourses: started");

        //Debug.Log("Number of subscribed items: " + SteamUGC.GetNumSubscribedItems());
        var nItems = SteamUGC.GetNumSubscribedItems();
        if (nItems > 0)
        {
            PublishedFileId_t[] PublishedFileID = new PublishedFileId_t[nItems];
            uint ret = SteamUGC.GetSubscribedItems(PublishedFileID, nItems);

            //We want to iterate through all the file IDs in order to get their install info (folder out: Returns the absolute path to the folder containing the content by copying it) HOWEVER it only has the path if k_EItemStateInstalled is set
            foreach (PublishedFileId_t i in PublishedFileID)
            {
                uint state = SteamUGC.GetItemState(i);
                EItemState itemState = (EItemState)state;
                Debug.Log("SyncSubscribedToCourses: item state for: " + i.ToString() + " is: " + state.ToString() + ":" + itemState.ToString());

                if (itemState.HasFlag(EItemState.k_EItemStateNeedsUpdate))
                {
                    Debug.Log("SyncSubscribedToCourses: item state is greater than 5, indicating that it needs to be updateD?");
                    //bool download = SteamUGC.DownloadItem(i, true);
                    //if (download)
                    //{
                    //    SteamAPICall_t handle = SteamRemoteStorage.GetPublishedFileDetails(publishedFileID, 0);
                    //    DownloadItemResult.Set(handle);
                    //}
                    //continue;
                }

                //SteamAPICall_t handle = SteamUGC.RequestUGCDetails(i, 5);
                //installedStuff = SteamUGC.GetItemInstallInfo(i, out ulong SizeOnDisk, out string Folder, 1024, out uint punTimeStamp); //Must name the outs exactly the same as in docs, it returned null with folder, but making it Folder works
                //                                                                                                                       //print(Folder);
                //string[] path = Directory.GetFiles(Folder);
                //string filename = Path.GetFileName(path[0]);
                //string fullPath = path[0];
                //string destPath = Application.persistentDataPath + "/" + filename;

                //if (File.GetLastWriteTime(fullPath) > File.GetLastWriteTime(destPath))
                //{
                //    Debug.Log("SyncSubscribedToCourses: copying: " + fullPath + " to " + destPath);
                //    //print(destPath);
                //    System.IO.File.Copy(fullPath, destPath, true);
                //}
                MoveFile(i);


            }           
        }
        Debug.Log("SyncSubscribedToCourses: done");
    }
    void MoveFile(PublishedFileId_t i)
    {
        try
        {
            installedStuff = SteamUGC.GetItemInstallInfo(i, out ulong SizeOnDisk, out string Folder, 1024, out uint punTimeStamp); //Must name the outs exactly the same as in docs, it returned null with folder, but making it Folder works
                                                                                                                                   //print(Folder);

            Debug.Log("MoveFile: installedStuff: " + installedStuff.ToString());
            string[] path = Directory.GetFiles(Folder);
            if (path.Length <= 0)
            {
                Debug.Log("MoveFile: path length is 0? " + path.Length.ToString());
                return;
            }
            string filename = Path.GetFileName(path[0]);
            string fullPath = path[0];
            string destPath = Application.persistentDataPath + "/" + filename;

            if (File.GetLastWriteTime(fullPath) > File.GetLastWriteTime(destPath))
            {
                Debug.Log("MoveFile: copying: " + fullPath + " to " + destPath);
                //print(destPath);
                System.IO.File.Copy(fullPath, destPath, true);
            }
        }
        catch (Exception e)
        {
            Debug.Log("MoveFile: could not move file. Error: " + e);
        }
        
    }
    public void DownloadNewCourse(ulong courseWorkshopID)
    {
        if (courseWorkshopID == 0)
            return;

        //bool ret = SteamUGC.DownloadItem(new PublishedFileId_t(courseWorkshopID),true);
        //if (ret)
        //{

        //}

        SteamAPICall_t call = SteamUGC.SubscribeItem(new PublishedFileId_t(courseWorkshopID));
        SubscribeItemResult.Set(call);


        //SteamAPICall_t handle = SteamRemoteStorage.GetPublishedFileDetails(new PublishedFileId_t(courseWorkshopID), 0);
        //RemoteStorageGetPublishedFileDetailsResult.Set(handle);

        //bool ret = SteamUGC.SubscribeItem(new PublishedFileId_t(courseWorkshopID));
    }

    private void OnDownloadItemResult(DownloadItemResult_t pCallback, bool bIOFailure)
    {
        Debug.Log("OnDownloadItemResult: result: " + pCallback.m_eResult.ToString());
        //MoveFile(pCallback.m_nPublishedFileId);
        //this.GetComponent<CustomGolfCourseLoader>().NewCustomCourseFinishedDownloading();
        if (pCallback.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log("OnDownloadItemResult: result ok");
        }
        else
        {
            Debug.Log("OnDownloadItemResult: result failure?");
        }
    }
    void OnRemoteStorageGetPublishedFileDetailsResult(RemoteStorageGetPublishedFileDetailsResult_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log("OnRemoteStorageGetPublishedFileDetailsResult: file details: " + pCallback.m_pchFileName + ":" + pCallback.m_nFileSize.ToString() + ":" + pCallback.m_nPublishedFileId.ToString());
            //This is where we actually make the callback to download it
            UGCHandle = pCallback.m_hFile;

            SteamAPICall_t handle = SteamRemoteStorage.UGCDownload(UGCHandle, 0);
            RemoteStorageDownloadUGCResult.Set(handle);
        }
    }
    void OnRemoteStorageDownloadUGCResult(RemoteStorageDownloadUGCResult_t pCallback, bool bIOFailure)
    {
        //finally downloading the file
        byte[] Data = new byte[pCallback.m_nSizeInBytes];
        int ret = SteamRemoteStorage.UGCRead(UGCHandle, Data, pCallback.m_nSizeInBytes, 0, EUGCReadAction.k_EUGCRead_Close);

        itemContent = System.Text.Encoding.UTF8.GetString(Data, 0, ret);
        //File.WriteAllText(pCallback.m_pchFileName, itemContent);
        Debug.Log("OnRemoteStorageDownloadUGCResult: File written? " + pCallback.m_eResult.ToString() + " " + pCallback.m_pchFileName + " content? " + itemContent);

        //string destPath = Application.persistentDataPath + "/" + pCallback.m_pchFileName;
        //Debug.Log("OnRemoteStorageDownloadUGCResult: downloading file: " + pCallback.m_pchFileName + " content? " + itemContent);

        ////File.WriteAllText(destPath, itemContent);
        //FileStream fileStream = new FileStream(destPath, FileMode.Create);

        //using (StreamWriter writer = new StreamWriter(fileStream))
        //{
        //    writer.Write(itemContent);
        //}

        // get the newly subscribed course?
        //this.GetComponent<CustomGolfCourseLoader>().GetAllAvailableCourses(true);

        StartCoroutine(DelayForCourseDownload());
        //SyncSubscribedToCourses();
        //this.GetComponent<CustomGolfCourseLoader>().NewCustomCourseFinishedDownloading();
        //if (SceneManager.GetActiveScene().name == "Golf-prototype-topdown")
        //    GameObject.FindGameObjectWithTag("LocalNetworkPlayer").GetComponent<NetworkPlayer>().CustomCourseAdded();

    }
    IEnumerator DelayForCourseDownload()
    {
        yield return new WaitForSecondsRealtime(2.5f);
        SyncSubscribedToCourses();
        this.GetComponent<CustomGolfCourseLoader>().NewCustomCourseFinishedDownloading();
        if (SceneManager.GetActiveScene().name == "Golf-prototype-topdown")
            GameObject.FindGameObjectWithTag("LocalNetworkPlayer").GetComponent<NetworkPlayer>().CustomCourseAdded();
    }
    void OnRemoteStorageSubscribePublishedFileResult(RemoteStorageSubscribePublishedFileResult_t pCallback, bool bIOFailure)
    {
        if (!SteamManager.Initialized)
            return;
        Debug.Log("RemoteStorageSubscribePublishedFileResult: " + pCallback.m_eResult.ToString() + " " + pCallback.m_nPublishedFileId.ToString());

        if (pCallback.m_eResult == EResult.k_EResultOK)
        {
            // Get subscribed files?
            SteamAPICall_t handle = SteamRemoteStorage.EnumerateUserSubscribedFiles(0);
            RemoteStorageEnumerateUserSubscribedFilesResult.Set(handle);

            //SyncSubscribedToCourses();
            //if (SceneManager.GetActiveScene().name == "Golf-prototype-topdown")
            //    GameObject.FindGameObjectWithTag("LocalNetworkPlayer").GetComponent<NetworkPlayer>().CustomCourseAdded();
        }
    }
    void OnRemoteStorageEnumerateUserSubscribedFilesResult(RemoteStorageEnumerateUserSubscribedFilesResult_t pCallback, bool bIOFailure)
    {
        //Clear list from last call
        subscribedItemList = new List<PublishedFileId_t>();
        for (int i = 0; i < pCallback.m_nTotalResultCount; i++)
        {
            //fetch subscribed item and add it to the list
            PublishedFileId_t f = pCallback.m_rgPublishedFileId[i];
            subscribedItemList.Add(f);
            Debug.Log("OnRemoteStorageEnumerateUserSubscribedFilesResult: for " + f.ToString());
        }
        //Now that all files have been fetched we need to download them
        StartCoroutine(DownloadFiles());
        
    }
    IEnumerator DownloadFiles()
    {
        int dlItem = 0;
        while (dlItem < subscribedItemList.Count)
        {
            fetchedContent = false;
            GetItemContent(dlItem);
            while (fetchedContent == false)
            {
                yield return new WaitForEndOfFrame();
            }
            dlItem++;
        }
    }
    public void GetItemContent(int ItemID)
    {
        Debug.Log("GetItemContent: " + ItemID);
        publishedFileID = subscribedItemList[ItemID];
        SteamAPICall_t handle = SteamRemoteStorage.GetPublishedFileDetails(publishedFileID, 0);
        RemoteStorageGetPublishedFileDetailsResult.Set(handle);
    }
}
