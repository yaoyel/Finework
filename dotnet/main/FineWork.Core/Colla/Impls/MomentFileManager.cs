using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Avatar;
using FineWork.Colla.Checkers;
using FineWork.Common;
using FineWork.Files;

namespace FineWork.Colla.Impls
{
    public class MomentFileManager : AefEntityManager<MomentFileEntity, Guid>, IMomentFileManager
    {
        public MomentFileManager(ISessionProvider<AefSession> sessionProvider,
            IMomentManager mementManager,
            IFileManager fileManager) : base(sessionProvider)
        {
            Args.NotNull(mementManager, nameof(mementManager));
            Args.NotNull(fileManager, nameof(fileManager));

            this.m_MementManager = mementManager;
            this.m_FileMamager = fileManager;
        }

        private readonly IMomentManager m_MementManager;
        private readonly IFileManager m_FileMamager;


        private string GetMomentFileDirectory(MomentFileEntity momentFile,bool isThumbnail=false)
        {
            var path= $"moments/{momentFile.Moment.Id}/{momentFile.Id}";
            if (isThumbnail)
                path = string.Concat(path, "/", "thumbnail");
            return path;
        }

        private string GetMomentFileDirectory(MomentEntity moment)
        {
            return $"moments/{moment.Id.ToString().ToLower()}";
        }

        private string GetMomentBgImageDiectory(OrgEntity org)
        {
            return $"orgs/{org.Id}/momentbg";
        }

        public void CreateMementFile(Guid momentId, string contentType, Stream fileStream, string fileName,int momentFileCount=1)
        {
            if (fileStream == null) throw new ArgumentException("请选择要上传的文件");
            var moment = MomentExistsResult.Check(this.m_MementManager, momentId).ThrowIfFailed().Moment;
            var momentFile = new MomentFileEntity();

            momentFile.Id = Guid.NewGuid();
            momentFile.Moment = moment;
            momentFile.Name = fileName;
            momentFile.ContentType = contentType;

            m_FileMamager.CreateFile(GetMomentFileDirectory(momentFile), contentType, fileStream);
            if (moment.Type == MomentType.Image && contentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                UploadThumbnailForMement(momentFile, contentType, fileStream,momentFileCount);
            this.InternalInsert(momentFile);
        }


        private void UploadThumbnailForMement(MomentFileEntity momentFile, string contentType,Stream fileStream,int momentFileCount=1)
        {
            Args.NotNull(momentFile, nameof(momentFile));
            Args.NotEmpty(contentType, nameof(contentType));
            Args.NotNull(fileStream, nameof(fileStream)); 

            var cutedImage = ImageUtil.CutFromCenter(fileStream, 80, 80);

            m_FileMamager.CreateFile(GetMomentFileDirectory(momentFile, true), contentType, cutedImage);
        }

        public void UploadMomentBgImage(OrgEntity org, string contentType,Stream fileStream)
        {
            Args.NotNull(org, nameof(org));
            Args.NotEmpty(contentType, nameof(contentType));
            Args.NotNull(fileStream, nameof(fileStream));

            var compressImage = ImageUtil.CompressImage(fileStream, (int)AvatarSizes.Full);

            m_FileMamager.CreateFile(GetMomentBgImageDiectory(org), contentType, compressImage);

        }

        public void DeleteMementFileByMementId(Guid momentId)
        {
            var moment = MomentExistsResult.Check(this.m_MementManager, momentId).ThrowIfFailed().Moment;

            //删除库中数据
            var files = this.InternalFetch(p => p.Moment.Id == momentId).ToList();
            if (files.Any())
                files.ForEach(InternalDelete);

            this.m_FileMamager.DeleteFile(GetMomentFileDirectory(moment));
        }

        public void DownloadMomentFile(MomentFileEntity momentFile, Stream stream)
        {
            Args.NotNull(momentFile, nameof(momentFile));

            var pathName = GetMomentFileDirectory(momentFile);
            if (!m_FileMamager.FileIsExists(pathName))
                throw new FineWorkException("文件不存在。");

            m_FileMamager.DownloadToStream(pathName, stream);
        }


        public  MomentFileEntity FindMomentFileById(Guid momentFileId)
        {
            return this.InternalFind(momentFileId);
        }
    }
}