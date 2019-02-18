using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
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
            var cloudStorage = CloudStorageAccount.Parse(connectionString);
            var queueClient = cloudStorage.CreateCloudQueueClient();

            var blobClient = cloudStorage.CreateCloudBlobClient();
            blobContainer = blobClient.GetContainerReference("soundblob");
            blobContainer.CreateIfNotExists();

            cloudQueue = queueClient.GetQueueReference("soundqueue");
            cloudQueue.CreateIfNotExists();
        }

        protected void UploadButton_Click(object sender, EventArgs e)
        {
            UploadSoundFile();
        }

        private void UploadSoundFile()
        {
            if (soundFileUpload.HasFile)
            {
                var file = soundFileUpload.PostedFile;
                string ext = Path.GetExtension(file.FileName);
                if (FileExtensions.Any(e => e == ext))
                {
                    AddFileToQueue(file);
                }
                else
                {
                    messageLabel.Text = "Not valid extension";
                }
            }
        }

        private void AddFileToQueue(HttpPostedFile file)
        {
            string ext = Path.GetExtension(file.FileName);
            string filename = Guid.NewGuid().ToString() + ext;
            string blobName = "sounds/" + filename;

            var blob = blobContainer.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = file.ContentType;
            blob.UploadFromStream(file.InputStream);

            blob.Metadata["Title"] = file.FileName;
            blob.SetMetadata();

            byte[] bytes = Encoding.UTF8.GetBytes(filename);
            var message = new CloudQueueMessage(bytes);
            cloudQueue.AddMessage(message);
        }

        protected void RefreshButton_Click(object sender, EventArgs e)
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
            soundDisplayControl.DataSource = source;
            soundDisplayControl.DataBind();
        }
    }
}
