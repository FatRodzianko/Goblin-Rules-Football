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
    CustomGolfCourseLoader _customGolfCourseLoader;

    bool installedStuff;

    private Callback<DownloadItemResult_t> _downloadItemCallback;



    private CallResult<RemoteStorageSubscribePublishedFileResult_t> SubscribeItemResult;

    // Start is called before the first frame update
    private void Awake()
    {
        if (!_customGolfCourseLoader)
            _customGolfCourseLoader = this.GetComponent<CustomGolfCourseLoader>();
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable()
    {
        SubscribeItemResult = CallResult<RemoteStorageSubscribePublishedFileResult_t>.Create(OnRemoteStorageSubscribePublishedFileResult);

        _downloadItemCallback = Callback<DownloadItemResult_t>.Create(OnDownloaded);
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
                    bool download = SteamUGC.DownloadItem(i, true);
                    if (download)
                    {
                        Debug.Log("SyncSubscribedToCourses: download started?");
                    }
                    continue;
                }

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

        // subscribe to the item to start the download process
        SteamAPICall_t call = SteamUGC.SubscribeItem(new PublishedFileId_t(courseWorkshopID));
        SubscribeItemResult.Set(call);

    }
    private void OnDownloaded(DownloadItemResult_t callback)
    {
        Debug.Log("OnDownloaded: File id: " + callback.m_nPublishedFileId + " result? " + callback.m_eResult.ToString());

        if (callback.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log("OnDownloaded: result ok");
            uint state = SteamUGC.GetItemState(callback.m_nPublishedFileId);
            EItemState itemState = (EItemState)state;
            Debug.Log("OnDownloaded: item state for: " + callback.m_nPublishedFileId.ToString() + " is: " + state.ToString() + ":" + itemState.ToString());

            if (itemState.HasFlag(EItemState.k_EItemStateInstalled))
            {
                MoveFile(callback.m_nPublishedFileId);
                SyncSubscribedToCourses();
                _customGolfCourseLoader.NewCustomCourseFinishedDownloading();
                if (SceneManager.GetActiveScene().name == "Golf-prototype-topdown")
                    GameObject.FindGameObjectWithTag("LocalNetworkPlayer").GetComponent<NetworkPlayer>().CustomCourseAdded();
            }
        }

        
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
        Debug.Log("OnRemoteStorageSubscribePublishedFileResult: " + pCallback.m_eResult.ToString() + " " + pCallback.m_nPublishedFileId.ToString());

        if (pCallback.m_eResult == EResult.k_EResultOK)
        {
            // download the item. Will be received as DownloadItemResult_t callback in OnDownloaded
            bool download = SteamUGC.DownloadItem(pCallback.m_nPublishedFileId, true);
            if (download)
            {
                Debug.Log("OnRemoteStorageSubscribePublishedFileResult: download started?");
            }

        }
    }
    
}
