/*==============================================================*/
/* DBMS name:      Microsoft SQL Server 2012                    */
/* Created on:     2015/9/8 11:15:09                            */
/*==============================================================*/


create rule R_T_PartakerKind as
      @column in (0,1,2,3,4)
go

create rule R_T_ReviewStatus as
      @column in (0,1,2)
go

/*==============================================================*/
/* User: finework                                               */
/*==============================================================*/
create schema finework
go

/*==============================================================*/
/* Table: fw_AccountRoles                                       */
/*==============================================================*/
create table finework.fw_AccountRoles (
   AccountId            uniqueidentifier     not null,
   RoleId               uniqueidentifier     not null,
   constraint PK_fw_AccountRoles primary key (AccountId, RoleId)
)
go

/*==============================================================*/
/* Index: FK_AccountRoles_AccountId                             */
/*==============================================================*/




create nonclustered index FK_AccountRoles_AccountId on finework.fw_AccountRoles (AccountId ASC)
go

/*==============================================================*/
/* Index: FK_fw_AccountRoles2                                   */
/*==============================================================*/




create nonclustered index FK_fw_AccountRoles2 on finework.fw_AccountRoles (RoleId ASC)
go

/*==============================================================*/
/* Table: fw_Accounts                                           */
/*==============================================================*/
create table finework.fw_Accounts (
   Id                   uniqueidentifier     not null,
   Name                 nvarchar(32)         null,
   Password             varchar(254)         null,
   PasswordSalt         varchar(254)         null,
   PasswordFormat       varchar(254)         null,
   PasswordFailedCount  int                  null,
   Email                varchar(254)         null,
   IsEmailConfirmed     bit                  null,
   PhoneNumber          varchar(254)         null,
   IsPhoneNumberConfirmed bit                  null,
   IsTwoFactorEnabled   bit                  null,
   IsLocked             bit                  null,
   LockEndAt            datetime2            null,
   SecurityStamp        varchar(254)         null,
   CreatedAt            datetime2            not null,
   RowVer               timestamp            null,
   constraint PK_fw_Accounts primary key nonclustered (Id)
)
go

/*==============================================================*/
/* Table: fw_Claims                                             */
/*==============================================================*/
create table finework.fw_Claims (
   Id                   uniqueidentifier     not null,
   AccountId            uniqueidentifier     not null,
   ClaimType            varchar(254)         not null,
   ClaimValue           varchar(254)         not null,
   RowVer               timestamp            null,
   constraint PK_fw_Claims primary key nonclustered (Id)
)
go

/*==============================================================*/
/* Index: FK_Claims_AccountId                                   */
/*==============================================================*/




create nonclustered index FK_Claims_AccountId on finework.fw_Claims (AccountId ASC)
go

/*==============================================================*/
/* Table: fw_DeviceRegistrations                                */
/*==============================================================*/
create table finework.fw_DeviceRegistrations (
   Id                   uniqueidentifier     not null,
   AccountId            uniqueidentifier     not null,
   Platform             varchar(32)          null,
   PlatformDescription  varchar(254)         null,
   RegistrationId       varchar(32)          null,
   CreatedAt            datetime2            not null,
   RowVer               timestamp            null,
   constraint PK_fw_DeviceRegistrations primary key (Id)
)
go

/*==============================================================*/
/* Index: FK_fw_DeviceRegistrations_AccountId                   */
/*==============================================================*/




create nonclustered index FK_fw_DeviceRegistrations_AccountId on finework.fw_DeviceRegistrations (AccountId ASC)
go

/*==============================================================*/
/* Table: fw_Logins                                             */
/*==============================================================*/
create table finework.fw_Logins (
   Id                   uniqueidentifier     not null,
   AccountId            uniqueidentifier     not null,
   Provider             varchar(254)         not null,
   ProviderKey          varchar(254)         not null,
   RowVer               timestamp            null,
   constraint PK_fw_Logins primary key nonclustered (Id)
)
go

/*==============================================================*/
/* Index: FK_Logins_AccountId                                   */
/*==============================================================*/




create nonclustered index FK_Logins_AccountId on finework.fw_Logins (AccountId ASC)
go

/*==============================================================*/
/* Table: fw_Orgs                                               */
/*==============================================================*/
create table finework.fw_Orgs (
   Id                   uniqueidentifier     not null,
   AdminStaffId         uniqueidentifier     null,
   Name                 nvarchar(32)         not null,
   CreatedAt            datetime2            not null,
   IsInvEnabled         bit                  not null,
   RowVer               timestamp            null,
   constraint PK_fw_Orgs primary key nonclustered (Id)
)
go

/*==============================================================*/
/* Index: FK_Orgs_AdminStaffId                                  */
/*==============================================================*/




create nonclustered index FK_Orgs_AdminStaffId on finework.fw_Orgs (AdminStaffId ASC)
go

/*==============================================================*/
/* Table: fw_PartakerInvs                                       */
/*==============================================================*/
create table finework.fw_PartakerInvs (
   Id                   uniqueidentifier     not null,
   TaskId               uniqueidentifier     not null,
   StaffId              uniqueidentifier     not null,
   PartakerKind         int                  null 
      constraint CKC_fw_PartakerInvs_PartakerKind check (PartakerKind is null or (PartakerKind in (0,1,2,3,4))),
   Message              varchar(254)         null,
   InviterNames         varchar(254)         null,
   CreatedAt            datetime2            not null,
   ReviewStatus         int                  null 
      constraint CKC_fw_PartakerInvs_ReviewStatus check (ReviewStatus is null or (ReviewStatus in (0,1,2))),
   ReviewAt             datetime2            null,
   constraint PK_fw_PartakerInvs primary key nonclustered (Id)
)
go

/*==============================================================*/
/* Index: FK_TaskInvs_StaffId                                   */
/*==============================================================*/




create nonclustered index FK_TaskInvs_StaffId on finework.fw_PartakerInvs (StaffId ASC)
go

/*==============================================================*/
/* Index: FK_TaskInvs_TaskId                                    */
/*==============================================================*/




create nonclustered index FK_TaskInvs_TaskId on finework.fw_PartakerInvs (TaskId ASC)
go

/*==============================================================*/
/* Table: fw_PartakerReqs                                       */
/*==============================================================*/
create table finework.fw_PartakerReqs (
   Id                   uniqueidentifier     not null,
   TaskId               uniqueidentifier     not null,
   StaffId              uniqueidentifier     not null,
   PartakerKind         int                  null 
      constraint CKC_fw_PartakerReqs_PartakerKind check (PartakerKind is null or (PartakerKind in (0,1,2,3,4))),
   Message              varchar(254)         null,
   CreatedAt            datetime2            not null,
   ReviewStatus         int                  null 
      constraint CKC_fw_PartakerReqs_ReviewStatus check (ReviewStatus is null or (ReviewStatus in (0,1,2))),
   ReviewAt             datetime2            null,
   constraint PK_fw_PartakerReqs primary key (Id)
)
go

/*==============================================================*/
/* Index: FK_TaskReq_StaffId                                    */
/*==============================================================*/




create nonclustered index FK_TaskReq_StaffId on finework.fw_PartakerReqs (StaffId ASC)
go

/*==============================================================*/
/* Index: FK_TaskReqs_TaskId                                    */
/*==============================================================*/




create nonclustered index FK_TaskReqs_TaskId on finework.fw_PartakerReqs (TaskId ASC)
go

/*==============================================================*/
/* Table: fw_Partakers                                          */
/*==============================================================*/
create table finework.fw_Partakers (
   Id                   uniqueidentifier     not null,
   TaskId               uniqueidentifier     not null,
   StaffId              uniqueidentifier     not null,
   Kind                 int                  null 
      constraint CKC_KIND_FW_PARTA check (Kind is null or (Kind in (0,1,2,3,4))),
   CreatedAt            datetime2            not null,
   constraint PK_fw_Partakers primary key nonclustered (Id)
)
go

/*==============================================================*/
/* Index: FK_Partakers_StaffId                                  */
/*==============================================================*/




create nonclustered index FK_Partakers_StaffId on finework.fw_Partakers (StaffId ASC)
go

/*==============================================================*/
/* Index: FK_Partakers_TaskId                                   */
/*==============================================================*/




create nonclustered index FK_Partakers_TaskId on finework.fw_Partakers (TaskId ASC)
go

/*==============================================================*/
/* Table: fw_Roles                                              */
/*==============================================================*/
create table finework.fw_Roles (
   Id                   uniqueidentifier     not null,
   Name                 nvarchar(32)         not null,
   RowVer               timestamp            null,
   constraint PK_fw_Roles primary key nonclustered (Id)
)
go

/*==============================================================*/
/* Table: fw_Settings                                           */
/*==============================================================*/
create table finework.fw_Settings (
   Id                   uniqueidentifier     not null,
   Name                 varchar(254)         not null,
   Value                nvarchar(max)        null,
   constraint PK_fw_Settings primary key nonclustered (Id)
)
go

/*==============================================================*/
/* Table: fw_StaffInvs                                          */
/*==============================================================*/
create table finework.fw_StaffInvs (
   Id                   uniqueidentifier     not null,
   OrgId                uniqueidentifier     not null,
   AccountId            uniqueidentifier     not null,
   Message              varchar(254)         null,
   InviterNames         varchar(254)         null,
   CreatedAt            datetime2            not null,
   ReviewStatus         int                  null 
      constraint CKC_fw_StaffInvs_ReviewStatus check (ReviewStatus is null or (ReviewStatus in (0,1,2))),
   ReviewAt             datetime2            null,
   constraint PK_fw_StaffInvs primary key nonclustered (Id)
)
go

/*==============================================================*/
/* Index: FK_fw_StaffInvs_OrgId                                 */
/*==============================================================*/




create nonclustered index FK_fw_StaffInvs_OrgId on finework.fw_StaffInvs (OrgId ASC)
go

/*==============================================================*/
/* Index: FK_fw_StaffInvs_AccountId                             */
/*==============================================================*/




create nonclustered index FK_fw_StaffInvs_AccountId on finework.fw_StaffInvs (AccountId ASC)
go

/*==============================================================*/
/* Table: fw_StaffReqs                                          */
/*==============================================================*/
create table finework.fw_StaffReqs (
   Id                   uniqueidentifier     not null,
   AccountId            uniqueidentifier     not null,
   OrgId                uniqueidentifier     not null,
   Message              varchar(254)         null,
   CreatedAt            datetime2            not null,
   ReviewStatus         int                  null 
      constraint CKC_fw_StaffReqs_ReviewStatus check (ReviewStatus is null or (ReviewStatus in (0,1,2))),
   ReviewAt             datetime2            null,
   constraint PK_fw_StaffReqs primary key nonclustered (Id)
)
go

/*==============================================================*/
/* Index: FK_fw_StaffReqs_AccountId                             */
/*==============================================================*/




create nonclustered index FK_fw_StaffReqs_AccountId on finework.fw_StaffReqs (AccountId ASC)
go

/*==============================================================*/
/* Index: FK_fw_StaffReqs_OrgId                                 */
/*==============================================================*/




create nonclustered index FK_fw_StaffReqs_OrgId on finework.fw_StaffReqs (OrgId ASC)
go

/*==============================================================*/
/* Table: fw_Staffs                                             */
/*==============================================================*/
create table finework.fw_Staffs (
   Id                   uniqueidentifier     not null,
   OrgId                uniqueidentifier     not null,
   AccountId            uniqueidentifier     not null,
   Name                 nvarchar(32)         not null,
   CreatedAt            datetime2            not null,
   IsEnabled            bit                  null,
   LastVisitAt          datetime2            null,
   RowVer               timestamp            null,
   constraint PK_fw_Staffs primary key nonclustered (Id)
)
go

/*==============================================================*/
/* Index: FK_Staffs_AccountId                                   */
/*==============================================================*/




create nonclustered index FK_Staffs_AccountId on finework.fw_Staffs (AccountId ASC)
go

/*==============================================================*/
/* Index: FK_Staffs_OrgId                                       */
/*==============================================================*/




create nonclustered index FK_Staffs_OrgId on finework.fw_Staffs (OrgId ASC)
go

/*==============================================================*/
/* Table: fw_TaskAlarms                                         */
/*==============================================================*/
create table finework.fw_TaskAlarms (
   Id                   uniqueidentifier     not null,
   TaskId               uniqueidentifier     not null,
   StaffId              uniqueidentifier     not null,
   Content              varchar(254)         null,
   CreatedAt            datetime2            not null,
   IsResolved           bit                  null,
   ResolvedAt           datetime2            null,
   Comment              varchar(254)         null,
   constraint PK_fw_TaskAlarms primary key (Id)
)
go

/*==============================================================*/
/* Index: FK_TaskAlarms_StaffId                                 */
/*==============================================================*/




create nonclustered index FK_TaskAlarms_StaffId on finework.fw_TaskAlarms (StaffId ASC)
go

/*==============================================================*/
/* Index: FK_TaskAlarms_TaskId                                  */
/*==============================================================*/




create nonclustered index FK_TaskAlarms_TaskId on finework.fw_TaskAlarms (TaskId ASC)
go

/*==============================================================*/
/* Table: fw_TaskAnnouncements                                  */
/*==============================================================*/
create table finework.fw_TaskAnnouncements (
   Id                   uniqueidentifier     not null,
   TaskId               uniqueidentifier     not null,
   AnnounceKind         int                  null 
      constraint CKC_ANNOUNCEKIND_FW_TASKA check (AnnounceKind is null or (AnnounceKind in (1,2))),
   IsGoodNews           bit                  null,
   StaffId              uniqueidentifier     not null,
   Message              varchar(254)         null,
   CreatedAt            datetime2            not null,
   RowVer               timestamp            null,
   constraint PK_fw_TaskAnnounces primary key nonclustered (Id)
)
go

/*==============================================================*/
/* Index: FK_TaskAnnouncements_TaskId                           */
/*==============================================================*/




create nonclustered index FK_TaskAnnouncements_TaskId on finework.fw_TaskAnnouncements (TaskId ASC)
go

/*==============================================================*/
/* Index: FK_TaskAnnouncements_StaffId                          */
/*==============================================================*/




create nonclustered index FK_TaskAnnouncements_StaffId on finework.fw_TaskAnnouncements (StaffId ASC)
go

/*==============================================================*/
/* Table: fw_TaskLogs                                           */
/*==============================================================*/
create table finework.fw_TaskLogs (
   Id                   uniqueidentifier     not null,
   TaskId               uniqueidentifier     not null,
   CreatedAt            datetime2            not null,
   StaffId              varchar(254)         null,
   TargetKind           varchar(254)         null,
   TargetId             uniqueidentifier     null,
   ActionKind           varchar(254)         null,
   Message              varchar(254)         null,
   constraint PK_FW_TASKLOGS primary key (Id)
)
go

/*==============================================================*/
/* Index: FK_TaskLogs_TaskId                                    */
/*==============================================================*/




create nonclustered index FK_TaskLogs_TaskId on finework.fw_TaskLogs (TaskId ASC)
go

/*==============================================================*/
/* Table: fw_Tasks                                              */
/*==============================================================*/
create table finework.fw_Tasks (
   Id                   uniqueidentifier     not null,
   CreatorStaffId       uniqueidentifier     not null,
   ParentTaskId         uniqueidentifier     null,
   Name                 nvarchar(32)         not null,
   Goal                 varchar(254)         null,
   Level                int                  null default 3
      constraint CKC_LEVEL_FW_TASKS check (Level is null or (Level between 1 and 5)),
   Progress             int                  not null default 0,
   IsMentorInvEnabled   bit                  null,
   IsCollabratorInvEnabled bit                  null,
   IsLeaderReqEnabled   bit                  null,
   IsMentorReqEnabled   bit                  null,
   IsCollabratorReqEnabled bit                  null,
   CreatedAt            datetime2            not null,
   RowVer               timestamp            null,
   constraint PK_fw_Tasks primary key nonclustered (Id)
)
go

/*==============================================================*/
/* Index: FK_Tasks_ParentTaskId                                 */
/*==============================================================*/




create nonclustered index FK_Tasks_ParentTaskId on finework.fw_Tasks (ParentTaskId ASC)
go

/*==============================================================*/
/* Index: FK_Tasks_CreatorId                                    */
/*==============================================================*/




create nonclustered index FK_Tasks_CreatorId on finework.fw_Tasks (CreatorStaffId ASC)
go

alter table finework.fw_AccountRoles
   add constraint FK_fw_AccountRoles_AccountId foreign key (AccountId)
      references finework.fw_Accounts (Id)
go

alter table finework.fw_AccountRoles
   add constraint FK_fw_AccountRoles_RoleId foreign key (RoleId)
      references finework.fw_Roles (Id)
go

alter table finework.fw_Claims
   add constraint FK_fw_Claims_AccountId foreign key (AccountId)
      references finework.fw_Accounts (Id)
go

alter table finework.fw_DeviceRegistrations
   add constraint FK_fw_DeviceRegistrations_AccountId foreign key (AccountId)
      references finework.fw_Accounts (Id)
go

alter table finework.fw_Logins
   add constraint FK_fw_Logins_AccountId foreign key (AccountId)
      references finework.fw_Accounts (Id)
go

alter table finework.fw_Orgs
   add constraint FK_fw_Orgs_AdminStaffId foreign key (AdminStaffId)
      references finework.fw_Staffs (Id)
go

alter table finework.fw_PartakerInvs
   add constraint FK_fw_TaskInvs_StaffId foreign key (StaffId)
      references finework.fw_Staffs (Id)
go

alter table finework.fw_PartakerInvs
   add constraint FK_fw_TaskInvs_TaskId foreign key (TaskId)
      references finework.fw_Tasks (Id)
go

alter table finework.fw_PartakerReqs
   add constraint FK_fw_TaskReqs_StaffId foreign key (StaffId)
      references finework.fw_Staffs (Id)
go

alter table finework.fw_PartakerReqs
   add constraint FK_fw_TaskReqs_TaskId foreign key (TaskId)
      references finework.fw_Tasks (Id)
go

alter table finework.fw_Partakers
   add constraint FK_fw_Partakers_StaffId foreign key (StaffId)
      references finework.fw_Staffs (Id)
go

alter table finework.fw_Partakers
   add constraint FK_fw_Partakers_TaskId foreign key (TaskId)
      references finework.fw_Tasks (Id)
go

alter table finework.fw_StaffInvs
   add constraint FK_fw_StaffInvs_AccountId foreign key (AccountId)
      references finework.fw_Accounts (Id)
go

alter table finework.fw_StaffInvs
   add constraint FK_fw_StaffInvs_OrgId foreign key (OrgId)
      references finework.fw_Orgs (Id)
go

alter table finework.fw_StaffReqs
   add constraint FK_fw_OrgApplys_AccountId foreign key (AccountId)
      references finework.fw_Accounts (Id)
go

alter table finework.fw_StaffReqs
   add constraint FK_fw_OrgApplys_OrgId foreign key (OrgId)
      references finework.fw_Orgs (Id)
go

alter table finework.fw_Staffs
   add constraint FK_fw_Staffs_AccountId foreign key (AccountId)
      references finework.fw_Accounts (Id)
go

alter table finework.fw_Staffs
   add constraint FK_fw_Staffs_OrgId foreign key (OrgId)
      references finework.fw_Orgs (Id)
go

alter table finework.fw_TaskAlarms
   add constraint FK_fw_TaskAlarms_StaffId foreign key (StaffId)
      references finework.fw_Staffs (Id)
go

alter table finework.fw_TaskAlarms
   add constraint FK_fw_TaskAlarms_TaskId foreign key (TaskId)
      references finework.fw_Tasks (Id)
go

alter table finework.fw_TaskAnnouncements
   add constraint FK_fw_TaskAnnouncements_StaffId foreign key (StaffId)
      references finework.fw_Staffs (Id)
go

alter table finework.fw_TaskAnnouncements
   add constraint FK_fw_TaskAnnouncements_TaskId foreign key (TaskId)
      references finework.fw_Tasks (Id)
go

alter table finework.fw_TaskLogs
   add constraint FK_fw_TaskLogs_TaskId foreign key (TaskId)
      references finework.fw_Tasks (Id)
go

alter table finework.fw_Tasks
   add constraint FK_fw_Tasks_CreatorId foreign key (CreatorStaffId)
      references finework.fw_Staffs (Id)
go

alter table finework.fw_Tasks
   add constraint FK_fw_Tasks_ParentTaskId foreign key (ParentTaskId)
      references finework.fw_Tasks (Id)
go

