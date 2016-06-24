SET XACT_ABORT ON

--BEGIN TRANSACTION

/*------------------------------------------------------------------------------
	Drop StoredProcedures/Functions
------------------------------------------------------------------------------*/

--DROP PROCEDURE ;
--DROP FUNCTION ;

/*------------------------------------------------------------------------------
	Drop Rules
------------------------------------------------------------------------------*/

/*------------------------------------------------------------------------------
	Drop Views
------------------------------------------------------------------------------*/

/*------------------------------------------------------------------------------
	Drop tables in their dependency order
------------------------------------------------------------------------------*/
PRINT 'DROP TABLE finework.fw_PartakerReqs';
DROP TABLE finework.fw_PartakerReqs;

PRINT 'DROP TABLE finework.fw_PartakerInvs';
DROP TABLE finework.fw_PartakerInvs;

PRINT 'DROP TABLE finework.fw_Partakers';
DROP TABLE finework.fw_Partakers;

PRINT 'DROP TABLE finework.fw_TaskAnnounces';
DROP TABLE finework.fw_TaskAnnounces;

PRINT 'DROP TABLE finework.fw_TaskCommitments';
DROP TABLE finework.fw_TaskCommitments;

PRINT 'DROP TABLE finework.fw_TaskAlarms';
DROP TABLE finework.fw_TaskAlarms;

PRINT 'DROP TABLE finework.fw_Tasks';
DROP TABLE finework.fw_Tasks;

PRINT 'DROP TABLE finework.fw_Staffs';
DROP TABLE finework.fw_Staffs;

PRINT 'DROP TABLE finework.fw_Orgs';
DROP TABLE finework.fw_Orgs;

PRINT 'DROP TABLE finework.fw_AccountRoles'
DROP TABLE finework.fw_AccountRoles

PRINT 'DROP TABLE finework.fw_Roles'
DROP TABLE finework.fw_Roles

PRINT 'DROP TABLE finework.fw_Claims'
DROP TABLE finework.fw_Claims

PRINT 'DROP TABLE finework.fw_Logins'
DROP TABLE finework.fw_Logins

PRINT 'DROP TABLE finework.fw_Accounts';
DROP TABLE finework.fw_Accounts;

PRINT 'DROP TABLE finework.fw_Settings';
DROP TABLE finework.fw_Settings;

--COMMIT TRANSACTION