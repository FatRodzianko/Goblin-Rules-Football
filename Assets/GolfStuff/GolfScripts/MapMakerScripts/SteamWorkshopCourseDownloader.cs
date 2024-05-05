using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System.IO;
using UnityEngine.UI;

public class SteamWorkshopCourseDownloader : MonoBehaviour
{
    bool installedStuff;

    // Start is called before the first frame update
    void Start()
    {
        //SyncSubscribedToCourses();
    }

    // Update is called once per frame
    void Update()
    {
        
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
                //SteamAPICall_t handle = SteamUGC.RequestUGCDetails(i, 5);
                installedStuff = SteamUGC.GetItemInstallInfo(i, out ulong SizeOnDisk, out string Folder, 1024, out uint punTimeStamp); //Must name the outs exactly the same as in docs, it returned null with folder, but making it Folder works
                                                                                                                                       //print(Folder);
                string[] path = Directory.GetFiles(Folder);
                string filename = Path.GetFileName(path[0]);
                string fullPath = path[0];
                string destPath = Application.persistentDataPath + "/" + filename;

                if (File.GetLastWriteTime(fullPath) > File.GetLastWriteTime(destPath))
                {
                    Debug.Log("SyncSubscribedToCourses: copying: " + fullPath + " to " + destPath);
                    //print(destPath);
                    System.IO.File.Copy(fullPath, destPath, true);
                }
                
            }           
        }
        Debug.Log("SyncSubscribedToCourses: done");
    }


}
