
begin tran


------------------------------------------------------------------------------
IF NOT EXISTS(SELECT * FROM UserType WHERE UserTypeName = 'Admin')
BEGIN
	insert into UserType(UserTypeName) values('Admin')
END
IF NOT EXISTS(SELECT * FROM UserType WHERE UserTypeName = 'Staff')
BEGIN
	insert into UserType(UserTypeName) values('Staff')
END
IF NOT EXISTS(SELECT * FROM UserType WHERE UserTypeName = 'Referrer')
BEGIN
	insert into UserType(UserTypeName) values('Referrer')
END
GO
---------------------------------------------------------------------------------
IF NOT EXISTS(SELECT * FROM WebPages_Roles WHERE RoleName = 'Admin')
BEGIN
	insert into WebPages_Roles(RoleName) values('Admin')
END
IF NOT EXISTS(SELECT * FROM WebPages_Roles WHERE RoleName = 'Staff')
BEGIN
	insert into WebPages_Roles(RoleName) values('Staff')
END
IF NOT EXISTS(SELECT * FROM WebPages_Roles WHERE RoleName = 'Referrer')
BEGIN
	insert into WebPages_Roles(RoleName) values('Referrer')
END
GO
-------------------------------------------------------------------------------------

declare @userTypeId integer
set @userTypeId = (select UserTypeId from UserType where UserTypeName = 'Admin')

insert into userprofile(UserName, FirstName, LastName, UserTypeId, email) values('pacificadmin@gmail.com', 'pacificadmin', 'pacificadmin', @userTypeId, 'nitinv.verma@gmail.com')

declare @userId integer
set @userId = (select UserId from UserProfile)

insert into webpages_Membership values(@userId, GetDate(), null, 1, null, 0, 'AJ8PVnvGcpKQ26ZzRv8Ew7x+AZWhQMYZ28yCj0BtIiZIwayKAwBeUSAWjfVwOiHMrA==', null, '', null, null)

declare @roleId integer
set @roleId = (select RoleId from webpages_Roles where RoleName = 'Admin')

insert into webpages_usersInRoles values(@userId, @roleId)
GO
-------------------------------------------------------------------------------------


--CLASSIFICATION
IF NOT EXISTS(SELECT * FROM Classification WHERE ClassificationName = 'Social')
BEGIN
	INSERT INTO Classification(ClassificationName) VALUES('Social')
	INSERT INTO Classification(ClassificationName) VALUES('Learning')
	INSERT INTO Classification(ClassificationName) VALUES('Employment')
	INSERT INTO Classification(ClassificationName) VALUES('Soft Outcomes')
	Insert into Classification(ClassificationName) VALUES('Employment/Learning')
END

GO

--GENDER
IF NOT EXISTS(SELECT * FROM Gender WHERE GenderName = 'Male')
BEGIN
	INSERT INTO Gender(GenderName) VALUES('Male')
	INSERT INTO Gender(GenderName) VALUES('Female')
END
GO

--Funding responsibility
IF NOT EXISTS(SELECT * FROM FundingResponsibility WHERE FundingResponsibilityName = 'RBKC')
BEGIN
	INSERT INTO FundingResponsibility(FundingResponsibilityName) VALUES('RBKC')
	INSERT INTO FundingResponsibility(FundingResponsibilityName) VALUES('Westminster')
END
GO

--Living Area
IF NOT EXISTS(SELECT * FROM LivingArea WHERE LivingAreaName = 'RBKC-Central')
BEGIN
	INSERT INTO LivingArea(LivingAreaName) VALUES('RBKC-Central')
	INSERT INTO LivingArea(LivingAreaName) VALUES('RBKC-North')
	INSERT INTO LivingArea(LivingAreaName) VALUES('RBKC-South')
END
GO

--Ethnicity
IF NOT EXISTS(SELECT * FROM Ethnicity WHERE EthnicityName = 'British')
BEGIN
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('British', 1, 'White')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Irish', 1, 'White')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Gypsy or Irish Traveller', 1, 'White')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Other White Background', 1, 'White')
	--
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Black British', 2, 'Black / African / Caribbean / Black British')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('African', 2, 'Black / African / Caribbean / Black British')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Caribbean', 2, 'Black / African / Caribbean / Black British')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Any other Black background', 2, 'Black / African / Caribbean / Black British')
	---
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('White and Black Caribbean', 3, 'Mixed/Multiple ethnic groups')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('White and Black African', 3, 'Mixed/Multiple ethnic groups')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('White and Asian', 3, 'Mixed/Multiple ethnic groups')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Any other Mixed/ Multiple ethnic background', 3, 'Mixed/Multiple ethnic groups')
	----
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Indian', 4, 'Asian / Asian British')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Pakistani', 4, 'Asian / Asian British')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Bangladeshi', 4, 'Asian / Asian British')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Chinese', 4, 'Asian / Asian British')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Any other Asian Background', 4, 'Asian / Asian British')
	----
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Arab', 5, 'Other ethnic group')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Arab British', 5, 'Other ethnic group')
	INSERT INTO Ethnicity(EthnicityName, GroupId, GroupName) VALUES('Other', 5, 'Other ethnic group')
END
----
GO

GO
--Nominations
IF NOT EXISTS(SELECT * FROM Nomination WHERE NominationName = 'Member')
BEGIN
	INSERT INTO Nomination(NominationName) VALUES('Member')
	INSERT INTO Nomination(NominationName) VALUES('Old Member')
	INSERT INTO Nomination(NominationName) VALUES('New Member')
	INSERT INTO Nomination(NominationName) VALUES('Barred Member')
	INSERT INTO Nomination(NominationName) VALUES('Unsuccessful Referral')
	INSERT INTO Nomination(NominationName) VALUES('Social Member')
END
GO


--MemberRole
IF NOT EXISTS(SELECT * FROM MemberRole WHERE MemberRoleName = 'Volunteer')
BEGIN
	INSERT INTO MemberRole(MemberRoleName) VALUES('Volunteer')
	INSERT INTO MemberRole(MemberRoleName) VALUES('Paid')
	INSERT INTO MemberRole(MemberRoleName) VALUES('Trainee')
	INSERT INTO MemberRole(MemberRoleName) VALUES('Participant')
END
GO

--Sexual Orientation
IF NOT EXISTS(SELECT * FROM SexualOrientation WHERE SexualOrientationName = 'Bisexual')
BEGIN
	INSERT INTO SexualOrientation(SexualOrientationName) VALUES('Bisexual')
	INSERT INTO SexualOrientation(SexualOrientationName) VALUES('Gay Man')
	INSERT INTO SexualOrientation(SexualOrientationName) VALUES('Gay Woman / Lesbian')
	INSERT INTO SexualOrientation(SexualOrientationName) VALUES('Heterosexual / Straight')
	INSERT INTO SexualOrientation(SexualOrientationName) VALUES('Other')
	INSERT INTO SexualOrientation(SexualOrientationName) VALUES('Prefer not to say')
END
GO

IF NOT EXISTS(SELECT * FROM GroupByClause WHERE GroupByClauseText = 'Project')
BEGIN
	INSERT INTO GroupByClause(GroupByClauseText, SortOrder) VALUES('Project', 1)
	INSERT INTO GroupByClause(GroupByClauseText, SortOrder) VALUES('Gender', 2)
	INSERT INTO GroupByClause(GroupByClauseText, SortOrder) VALUES('Ethnicity', 3)
	INSERT INTO GroupByClause(GroupByClauseText, SortOrder) VALUES('Nomination', 4)
	INSERT INTO GroupByClause(GroupByClauseText, SortOrder) VALUES('AgeBracket', 5)
	INSERT INTO GroupByClause(GroupByClauseText, SortOrder) VALUES('FundingResponsibility', 6)
END
GO

IF NOT EXISTS(SELECT * FROM AgeBracket WHERE AgeBracketText = '18-29')
BEGIN
	INSERT INTO AgeBracket(AgeBracketText, SortOrder) VALUES('18-29', 1)
	INSERT INTO AgeBracket(AgeBracketText, SortOrder) VALUES('30-49', 2)
	INSERT INTO AgeBracket(AgeBracketText, SortOrder) VALUES('50-64', 3)
	INSERT INTO AgeBracket(AgeBracketText, SortOrder) VALUES('65+', 4)
END
GO


--ALTER TABLE member ALTER COLUMN Email NVARCHAR(100) NULL
--ALTER TABLE member ALTER COLUMN FundingResponsibilityId int NULL
--ALTER TABLE member ALTER COLUMN NominationId int NULL
--ALTER table Member Add RelationshipToClient nvarchar(200) NULL

GO
------------------------------------------------------------------------------------------------------------------------------

IF EXISTS(SELECT * FROM sys.Objects WHERE type = 'FN' and name = 'TimeAtSmart')
BEGIN
	DROP FUNCTION [dbo].[TimeAtSmart]
END
GO

CREATE FUNCTION [dbo].[TimeAtSmart]
(
	@StartDate DateTime
)
RETURNS NVARCHAR(200)
AS
BEGIN

	-- Declare the return variable here
	DECLARE @date datetime, @tmpdate datetime, @years int, @months int, @days int

	SELECT @date = @StartDate	--'1990-11-22'

	SELECT @tmpdate = @date

	SELECT @years = DATEDIFF(yy, @tmpdate, GETDATE()) - CASE WHEN (MONTH(@date) > MONTH(GETDATE())) OR (MONTH(@date) = MONTH(GETDATE()) AND DAY(@date) > DAY(GETDATE())) THEN 1 ELSE 0 END
	SELECT @tmpdate = DATEADD(yy, @years, @tmpdate)
	SELECT @months = DATEDIFF(m, @tmpdate, GETDATE()) - CASE WHEN DAY(@date) > DAY(GETDATE()) THEN 1 ELSE 0 END
	SELECT @tmpdate = DATEADD(m, @months, @tmpdate)
	SELECT @days = DATEDIFF(d, @tmpdate, GETDATE())



		-- Return the result of the function
		--RETURN <@ResultVar, sysname, @Result>

	RETURN Convert(nvarchar, @years) + ' Years ' + Convert(nvarchar, @months) + ' Months ' + Convert(nvarchar, @days) + ' Days'

END

GO


----------------------------------------------------------------------------------------------------------------------------------------------
IF EXISTS(SELECT * FROM sys.Objects WHERE type = 'FN' and name = 'CalculateAgeGroup')
BEGIN
	DROP FUNCTION [dbo].[CalculateAgeGroup]
END
GO

CREATE FUNCTION [dbo].[CalculateAgeGroup]
(
	@DateOfBirth DateTime
)
RETURNS NVARCHAR(200)
AS
BEGIN
	DECLARE @date datetime, @ageGroup int
	DECLARE @return NVARCHAR(200)

	SELECT @date = @DateOfBirth	--'1997-11-22'

	SELECT @ageGroup = DATEDIFF(year,  @date, GETDATE())

	IF(@ageGroup >= 18 and @ageGroup <= 29)
		 --SET @return = '18-29'
		 SELECT @return = AgeBracketId FROM AgeBracket WHERE AgeBracketText = '18-29'
	ELSE IF(@ageGroup >= 30 and @ageGroup <= 49)
		--SET @return =  '30-49'
		SELECT @return = AgeBracketId FROM AgeBracket WHERE AgeBracketText ='30-49'
	ELSE IF(@ageGroup >= 50 and @ageGroup <= 64)
		--SET @return =  '50-64'
		SELECT @return = AgeBracketId FROM AgeBracket WHERE AgeBracketText ='50-64'
	ELSE IF(@ageGroup >= 65)
		--SET @return =  '65+'
		SELECT @return = AgeBracketId FROM AgeBracket WHERE AgeBracketText ='65+'
	ELSE
		SET @return = -1	-- 'LESS THAN 18'
			
	RETURN @return

END

GO




-------------------------------------------------------------------------------------------------------------------------------------------
IF EXISTS(SELECT * FROM sys.Objects WHERE type = 'FN' and name = 'TotalMembers')
BEGIN
	DROP FUNCTION [dbo].[TotalMembers]
END
GO

CREATE FUNCTION [dbo].[TotalMembers]
()
RETURNS int

AS
BEGIN
	

	declare @cnt int
	select  @cnt = count(*) from Member
	return @cnt

END

GO

-----------------------------------------------------------------------------------------------------------------------------------

IF EXISTS(SELECT * FROM sys.Objects WHERE type = 'P' and name = 'GetParameterizedReport')
BEGIN
	DROP PROCEDURE [dbo].[GetParameterizedReport]
END
GO

CREATE PROCEDURE [dbo].[GetParameterizedReport] 
	@DateFrom DateTime,
	@DateTo DateTime,
	@NominationId INT,
	@ProjecId INT,
	@AgeBracket NVARCHAR(20),
	@GenderId INT,
	@EthnicityId INT,
	@FundingResponsibilityId INT,
	@GroupByClause NVARCHAR(50) = null
AS
BEGIN

	select  p.ProjectName,[dbo].[CalculateAgeGroup](m.DateOfBirth) As AgeGroup,g.GenderName,  e.EthnicityName, 
		f.FundingResponsibilityName, m.MemberName, n.NominationName, 
		u.Agency, m.StartDate, m.DateOfBirth, [dbo].[TimeAtSmart](m.StartDate) As TimeAtSmart, [dbo].[TotalMembers]() as TotalMembers
	from member m
	inner join Gender g on g.GenderId = m.GenderId
	inner join Nomination n on n.NominationId = m.NominationId
	inner join Ethnicity e on e.EthnicityId = m.EthnicityId
	inner join UserProfile u on u.UserId = m.AgentId
	inner join FundingResponsibility f on f.FundingResponsibilityId = m.FundingResponsibilityId
	inner join MemberToProject mtp on mtp.MemberId = m.MemberId and mtp.IsAssigned = 1
	inner join Project p on p.ProjectId = mtp.ProjectId
	WHERE m.StartDate BETWEEN @DateFrom AND @DateTo
		AND m.NominationId = CASE WHEN @NominationId <> 0 THEN @NominationId ELSE m.NominationId END
		AND p.ProjectId = CASE WHEN @ProjecId <> 0 THEN @ProjecId ELSE p.ProjectId END
		AND [dbo].[CalculateAgeGroup](m.DateOfBirth) = CASE WHEN @AgeBracket is null then [dbo].[CalculateAgeGroup](m.DateOfBirth) ELSE @AgeBracket END
		AND m.GenderId = CASE WHEN @GenderId <> 0 THEN @GenderId ELSE m.GenderId END
		AND m.EthnicityId = CASE WHEN @EthnicityId <> 0 THEN @EthnicityId ELSE m.EthnicityId END
		AND m.FundingResponsibilityId = CASE WHEN @FundingResponsibilityId <> 0 THEN @FundingResponsibilityId ELSE m.FundingResponsibilityId END  

END


GO





--commit tran
--rollback tran

