﻿IF OBJECT_ID('GetCampaigns', 'P') IS NOT NULL
	DROP PROCEDURE GetCampaigns
GO

CREATE PROCEDURE GetCampaigns
	@CurrentDate DATETIME,
	@Filter NVARCHAR(4000) = NULL,
	@CurrencyId INT = NULL,
	@SortColumn NVARCHAR(100) = NULL,
	@SortDirection NVARCHAR(50) = NULL,
	@Skip INT,
	@Take INT
AS
SET NOCOUNT ON
DECLARE @SQLQuery NVARCHAR(MAX)
DECLARE @ParamDefinition NVARCHAR(MAX)
DECLARE @TitleFilterPattern NVARCHAR(4000)
DECLARE @CampaignIdFilterPattern NVARCHAR(4000)

SET @SQLQuery = N'
SELECT
	CampaignTemp.Profit Profit,
	CampaignTemp.Last24HoursSold Last24HoursSold,
	CampaignRecord.Id Id,
	CampaignRecord.Title Title,
	CampaignRecord.ProductCountGoal Goal,
	CampaignRecord.ProductCountSold Sold,
	CampaignRecord.IsApproved IsApproved,
	CampaignRecord.EndDate EndDate,
	CampaignRecord.Alias Alias,
	CampaignRecord.IsActive IsActive,
	CampaignRecord.ProductMinimumGoal Minimum,
	CampaignRecord.StartDate CreatedDate,
	CampaignStatusRecord.Name Status,
	TeeyootUserPartRecord.PhoneNumber PhoneNumber,
	UserPartRecord.UserName Email,
	CurrencyRecord.ShortName Currency
FROM(
	SELECT
		CampaignRecord.Id Id,
		SUM(CASE WHEN
				OrderRecord.IsActive = 1
				AND OrderStatusRecord.Name != ''Cancelled''
				AND OrderStatusRecord.Name != ''Pending''
			THEN
				LinkOrderCampaignProductRecord.Count * (CampaignProductRecord.Price - CampaignProductRecord.BaseCost)
			ELSE
				0
			END) Profit,
		SUM(CASE WHEN
				OrderRecord.IsActive = 1
				AND OrderStatusRecord.Name != ''Cancelled''
				AND	OrderStatusRecord.Name != ''Pending''
				AND OrderRecord.Created >= DATEADD(HH, -24, @CurrentDate)
			THEN
				LinkOrderCampaignProductRecord.Count
			ELSE
				0
			END) Last24HoursSold
	FROM
		Teeyoot_Module_CampaignRecord CampaignRecord
		LEFT JOIN Teeyoot_Module_CampaignProductRecord CampaignProductRecord
		ON CampaignRecord.Id = CampaignProductRecord.CampaignRecord_Id
		LEFT JOIN Teeyoot_Module_LinkOrderCampaignProductRecord LinkOrderCampaignProductRecord
		ON CampaignProductRecord.Id = LinkOrderCampaignProductRecord.CampaignProductRecord_Id
		LEFT JOIN Teeyoot_Module_OrderRecord OrderRecord
		ON LinkOrderCampaignProductRecord.OrderRecord_Id = OrderRecord.Id
		LEFT JOIN Teeyoot_Module_OrderStatusRecord OrderStatusRecord
		ON OrderRecord.OrderStatusRecord_Id = OrderStatusRecord.Id
	WHERE
		CampaignRecord.WhenDeleted IS NULL
	GROUP BY
		CampaignRecord.Id
	) CampaignTemp
	JOIN Teeyoot_Module_CampaignRecord CampaignRecord
	ON CampaignTemp.Id = CampaignRecord.Id
	LEFT JOIN Teeyoot_Module_CampaignStatusRecord CampaignStatusRecord
	ON CampaignRecord.CampaignStatusRecord_Id = CampaignStatusRecord.Id
	LEFT JOIN Teeyoot_Module_TeeyootUserPartRecord TeeyootUserPartRecord
	ON CampaignRecord.TeeyootUserId = TeeyootUserPartRecord.Id
	LEFT JOIN Orchard_Users_UserPartRecord UserPartRecord
	ON TeeyootUserPartRecord.Id = UserPartRecord.Id
	LEFT JOIN Teeyoot_Module_CurrencyRecord CurrencyRecord
	ON CampaignRecord.CurrencyRecord_Id = CurrencyRecord.Id
WHERE 
	1 = 1'

IF @CurrencyId IS NOT NULL
BEGIN
	SET @SQLQuery = @SQLQuery + N'
	AND CampaignRecord.CurrencyRecord_Id = @CurrencyId'
END

IF @Filter IS NOT NULL
BEGIN
	SET @CampaignIdFilterPattern = '''' + @Filter + '%'''
	SET @TitleFilterPattern = '''%' + @Filter + '%'''

	SET @SQLQuery = @SQLQuery + N'
	AND(
		CAST(CampaignRecord.Id AS VARCHAR(20)) LIKE ' + @CampaignIdFilterPattern + 
		' OR CampaignRecord.Title LIKE ' + @TitleFilterPattern + 
	')'
END

IF @SortColumn IS NOT NULL
BEGIN
	SET @SQLQuery = @SQLQuery + N'
ORDER BY ' + 
	@SortColumn + N' ' + @SortDirection
END
ELSE
BEGIN
	SET @SQLQuery = @SQLQuery + N'
ORDER BY 
	CampaignRecord.Id ASC' 
END

SET @SQLQuery = @SQLQuery + N'
OFFSET
	@Skip ROWS
FETCH NEXT
	@Take ROWS ONLY'

SET @ParamDefinition = N'
	@CurrentDate DATETIME,
	@CurrencyId INT,
	@Skip INT,
	@Take INT'

EXECUTE sp_executesql @SQLQuery, @ParamDefinition,
	@CurrentDate,
	@CurrencyId,
	@Skip,
	@Take
GO

IF OBJECT_ID('GetCampaignsCount', 'P') IS NOT NULL
	DROP PROCEDURE GetCampaignsCount
GO

CREATE PROCEDURE GetCampaignsCount
	@Filter NVARCHAR(4000) = NULL,
	@CurrencyId INT = NULL
AS
SET NOCOUNT ON
DECLARE @SQLQuery NVARCHAR(MAX)
DECLARE @ParamDefinition NVARCHAR(MAX)
DECLARE @TitleFilterPattern NVARCHAR(4000)
DECLARE @CampaignIdFilterPattern NVARCHAR(4000)

SET @SQLQuery = N'
SELECT
	COUNT(*)
FROM
	Teeyoot_Module_CampaignRecord CampaignRecord
WHERE 
	CampaignRecord.WhenDeleted IS NULL'

IF @CurrencyId IS NOT NULL
BEGIN
	SET @SQLQuery = @SQLQuery + N'
	AND CampaignRecord.CurrencyRecord_Id = @CurrencyId'
END

IF @Filter IS NOT NULL
BEGIN
	SET @CampaignIdFilterPattern = '''' + @Filter + '%'''
	SET @TitleFilterPattern = '''%' + @Filter + '%'''

	SET @SQLQuery = @SQLQuery + N'
	AND(
		CAST(CampaignRecord.Id AS VARCHAR(20)) LIKE ' + @CampaignIdFilterPattern + 
		' OR CampaignRecord.Title LIKE ' + @TitleFilterPattern + 
	')'
END

SET @ParamDefinition = N'
	@CurrencyId INT'

EXECUTE sp_executesql @SQLQuery, @ParamDefinition,
	@CurrencyId
GO