using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace CloudCoursework1
{
    public partial class Default : System.Web.UI.Page
    {
        private CloudBlobContainer blobContainer;
        private CloudQueue cloudQueue;
        private static readonly string[] FileExtensions = { ".mp3" };

        protected void Page_Load(object sender, EventArgs e)
        {
            InitializeCloud();
            UpdateSoundFiles();
        }

        private void InitializeCloud()
        {
            string connectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            CloudStorageAccount cloudStorage = CloudStorageAccount.Parse(connectionString);
            CloudQueueClient queueClient = cloudStorage.CreateCloudQueueClient();

            CloudBlobClient blobClient = cloudStorage.CreateCloudBlobClient();
            blobContainer = blobClient.GetContainerReference("soundblob");
            blobContainer.CreateIfNotExists();

            cloudQueue = queueClient.GetQueueReference("soundqueue");
            cloudQueue.CreateIfNotExists();
        }

        protected void uploadButton_Click(object sender, EventArgs e)
        {
            if (soundFileUpload.HasFile)
            {
                AddSoundFileToQueue(soundFileUpload.PostedFile);
            }
        }

        private void AddSoundFileToQueue(HttpPostedFile file)
        {
            string ext = Path.GetExtension(file.FileName);
            if (FileExtensions.Any(e => e == ext))
            {
                string name = Guid.NewGuid().ToString() + ext;

                CloudBlockBlob blob = blobContainer.GetBlockBlobReference("sounds/" + name);
                blob.Properties.ContentType = file.ContentType;
                blob.UploadFromStream(file.InputStream);
                blob.Metadata["Title"] = file.FileName;
                blob.SetMetadata();

                byte[] bytes = Encoding.UTF8.GetBytes(name);
                CloudQueueMessage message = new CloudQueueMessage(bytes);
                cloudQueue.AddMessage(message);
            }
            else
            {
                Debug.WriteLine("Not valid extension");
            }
        }

        protected void refreshButton_Click(object sender, EventArgs e)
        {
            UpdateSoundFiles();
        }

        private void UpdateSoundFiles()
        {
            var blobs = blobContainer.GetDirectoryReference("samples").ListBlobs();
            var source = blobs.Select(o =>
            {
                CloudBlockBlob blob = new CloudBlockBlob(o.Uri);
                blob.FetchAttributes();
                return new { Url = o.Uri, Title = blob.Metadata["Title"] };
            });
            sampleDisplayControl.DataSource = source;
            sampleDisplayControl.DataBind();
        }
    }
}
