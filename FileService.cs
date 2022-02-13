using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sabio.Data;
using Sabio.Data.Providers;
using Sabio.Models;
using Sabio.Models.AppSettings;
using Sabio.Models.Domain;
using Sabio.Models.Requests;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Sabio.Services
{
    public class FileService : IFileService
    {
        IDataProvider _data = null;
        private AwsKeys awsKeys;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USWest2;
        private static IAmazonS3 s3Client;

        public FileService(IDataProvider data, IOptions<AwsKeys> _awsKeys)
        {
            _data = data;
            awsKeys = _awsKeys.Value;
        }


        public async Task<List<File>> Upload(List<IFormFile> files, int userId)
        {
            Dictionary<string, string> uploads = null; 
            List<File> uploadFiles = null;

            using (s3Client = new AmazonS3Client(awsKeys.AccessKey, awsKeys.Secret, bucketRegion))
            {
                TransferUtility fileTransferUtility = new TransferUtility(s3Client);
                foreach (var item in files)
                {
                    string keyName = "welrus/" + Guid.NewGuid() + "-" + item.FileName;
                    await fileTransferUtility.UploadAsync(item.OpenReadStream(), awsKeys.BucketName, keyName);
                    string url = awsKeys.Domain + keyName;

                    if (uploads == null)
                    {
                        uploads = new Dictionary<string, string>();
                    }
                    uploads.Add(url, item.FileName);
                }
            }
            if (uploads != null)
            {
                uploadFiles = AddMultiple(uploads, userId);
            }
            return uploadFiles;
        }

        
        public List<File> AddMultiple(Dictionary<string, string> uploads, int userId)
        {
            List<File> response = null;

            string procName = "dbo.Files_Insert_V2";

            DataTable fileMappedValues = MapBatchUpload(uploads);

            _data.ExecuteCmd(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@userId", userId);
                col.AddWithValue("@batchFiles", fileMappedValues);

            }, singleRecordMapper: delegate (IDataReader reader, short set) 
            {
                File aFile = Mapper(reader, out int index);

                if (response == null)
                {
                    response = new List<File>();
                }
                response.Add(aFile);
            });
            return response;
        }


        private static DataTable MapBatchUpload(Dictionary<string, string> uploads)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Url", typeof(string));
            dt.Columns.Add("FileName", typeof(string));
            dt.Columns.Add("FileTypeId", typeof(int));

            foreach (KeyValuePair<string, string> upload in uploads)
            {
                DataRow dataRow = dt.NewRow();
                dataRow.SetField(0, upload.Key);
                dataRow.SetField(1, upload.Value);
                dataRow.SetField(2, FilterFileType(upload.Key));

                dt.Rows.Add(dataRow);
            }
            return dt;
        }


        private static int FilterFileType(string url)
        {
            string fileType = url[url.LastIndexOf(".")..];
            int fileTypeId = 0;

            switch (fileType.ToLower())
            {
                case ".jpg":
                    fileTypeId = 1;
                    break;
                case ".docx":
                    fileTypeId = 2;
                    break;
                case ".doc":
                    fileTypeId = 3;
                    break;
                case ".pdf":
                    fileTypeId = 4;
                    break;
                case ".png":
                    fileTypeId = 5;
                    break;
                case ".txt":
                    fileTypeId = 6;
                    break;                
                case ".gif":
                    fileTypeId = 10;
                    break;
                case ".svg":
                    fileTypeId = 11;
                    break;
                case ".bmp":
                    fileTypeId = 12;
                    break;
                case ".jpeg":
                    fileTypeId = 13;
                    break;
                default:
                    fileTypeId = 14;
                    break;
            }
            return fileTypeId;
        }


        public Paged<File> GetPaginate(int pageIndex, int pageSize)
        {
            Paged<File> pagedFiles = null;
            List<File> filesList = null;
            int totalCount = 0;

            string procName = "[dbo].[Files_SelectAll_V2]";

            _data.ExecuteCmd(procName, inputParamMapper: delegate (SqlParameterCollection model)
            {
                model.AddWithValue("@pageIndex", pageIndex);
                model.AddWithValue("@pageSize", pageSize);
            },
             singleRecordMapper: delegate (IDataReader reader, short set)
             {
                 File aFile = Mapper(reader, out int index);

                 if (totalCount == 0)
                 {
                     totalCount = reader.GetSafeInt32(index);
                 }

                 if (filesList == null)
                 {
                     filesList = new List<File>();
                 }
                 filesList.Add(aFile);

                 if (filesList != null)
                 {
                     pageSize = totalCount;
                     pagedFiles = new Paged<File>(filesList, pageIndex, pageSize, totalCount);
                 }
             });
            return pagedFiles;
        }


        public File GetById(int id)
        {
            string procName = "[dbo].[Files_SelectById_V2]";

            File aFile = null;

            _data.ExecuteCmd(procName, delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);

            }, delegate (IDataReader reader, short set)
            {
                aFile = Mapper(reader, out int index);

            });
            return aFile;
        }


        public Paged<File> GetByCreatedBy(int pageIndex, int pageSize, int userId)
        {
            Paged<File> pagedFiles = null;
            List<File> filesList = null;
            int totalCount = 0;

            string procName = "[dbo].[Files_SelectBy_CreatedBy_V2]";

            _data.ExecuteCmd(procName, inputParamMapper: delegate (SqlParameterCollection model)
            {
                model.AddWithValue("@CreatedBy", userId);
                model.AddWithValue("@pageIndex", pageIndex);
                model.AddWithValue("@pageSize", pageSize);

            },
             singleRecordMapper: delegate (IDataReader reader, short set)
             {
                 File aFile = Mapper(reader, out int index);

                 if (totalCount == 0)
                 {
                     totalCount = reader.GetSafeInt32(index);
                 }

                 if (filesList == null)
                 {
                     filesList = new List<File>();
                 }
                 filesList.Add(aFile);

                 if (filesList != null)
                 {
                     pageSize = totalCount;
                     pagedFiles = new Paged<File>(filesList, pageIndex, pageSize, totalCount);
                 }
             });
            return pagedFiles;
        }


        public void Update(FileUpdateRequest model, int userId)
        {
            string procName = "[dbo].[Files_Update]";
            _data.ExecuteNonQuery(procName,
            inputParamMapper: delegate (SqlParameterCollection collection)
            {
                collection.AddWithValue("@Id", model.Id);
                AddCommonParams(model, collection);
                collection.AddWithValue("@CreatedBy", userId);

            },
            returnParameters: null);
        }

        public void Delete(int id)
        {
            string procName = "[dbo].[Files_DeleteById]";

            _data.ExecuteNonQuery(procName,
            delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", id);
            });
        }


        private static void AddCommonParams(FileAddRequest model, SqlParameterCollection collection)
        {
            collection.AddWithValue("@Url", model.Url);
            collection.AddWithValue("@FileTypeId", model.FileTypeId);
        }


        private static File Mapper(IDataReader reader, out int startingIndex)
        {
            startingIndex = 0;

            File aFile = new File();
            aFile.Id = reader.GetSafeInt32(startingIndex++);
            aFile.Url = reader.GetSafeString(startingIndex++);
            aFile.FileName = reader.GetSafeString(startingIndex++);
            aFile.FileType = reader.GetSafeString(startingIndex++);
            aFile.CreatedBy = reader.GetSafeInt32(startingIndex++);
            aFile.DateCreated = reader.GetSafeDateTime(startingIndex++);

            return aFile;
        }

    }
}