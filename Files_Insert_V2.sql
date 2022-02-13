USE [Welrus]
GO
/****** Object:  StoredProcedure [dbo].[Files_Insert_V2]    Script Date: 2/12/2022 2:01:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Andy Hong
-- Create date: 12.4.2020

-- MODIFIED BY: Andy Hong
-- MODIFIED DATE: 12/15/20
-- Note: This proc will accept a userId(CreatedBy) from .NET, and a batch insert which consists of Url and FileTypeId
-- =============================================

ALTER proc [dbo].[Files_Insert_V2]
			@UserId int
		   ,@batchFiles as dbo.Files_BatchV3 Readonly

as

/*
=============================================
	DECLARE
			 @userId INT = 8
			,@batchFiles as dbo.Files_BatchV3

			insert into @batchFiles (Url, FileName, FileTypeId) 
			Values
			 ('https://www.example.com/test1.jpg', 'exampleFileName1', 2)
			,('https://www.example.com/test3.jpg', 'exampleFileName2', 4)
			,('https://www.example.com/test2.jpg', 'exampleFileName3', 5)

	EXECUTE dbo.[Files_Insert_V2]
			 @userId
			,@batchFiles 

	select * from dbo.files
	select * from dbo.fileTypes
=============================================
*/

BEGIN
--Initialize a temporary table @ReturnedIds; this will be returned later as a JSON column
DECLARE @ReturnedValues TABLE
   ([ReturnedId] INT NOT NULL, 
	[Url] NVARCHAR(255) NOT NULL,
	[FileName] nvarchar(100) NOT NULL)

----------------------------------------------
--Perform regular insert into Files with the 
INSERT INTO dbo.Files
        ([Url]
		,[FileName]
		,[FileTypeId]
		,[CreatedBy])

--https://www.dotnettricks.com/learn/sqlserver/inserted-deleted-logical-table-in-sql-server
--Syntax dictates INSERTED should be between INSERT INTO and SELECT ( line 74 )
--Does it need to be right below Insertion proc as syntax?
OUTPUT  INSERTED.[Id]
       ,INSERTED.[Url]
	   ,INSERTED.[FileName]


--Direct the output INSERTED Id and Url into the @ReturnedIds table
INTO @ReturnedValues
	 ([ReturnedId]
	 ,[Url]
	 ,[FileName])

--Inserted into Files
SELECT  fBatch.[Url],
		fBatch.[FileName],
        fBatch.[FileTypeId], 
        @UserId
FROM @batchFiles AS fBatch;
----------------------------------------------

--This is a SELECT statement that will return the JSON format of id and Url that was inserted into dbo.Files
SELECT 
	  ReturnedValues.[ReturnedId] AS Id
     ,Returnedvalues.[Url] AS Url
	 ,ReturnedValues.[FileName] AS FileName
	 ,(SELECT ft.[Name] AS FileType

	 FROM dbo.FileTypes AS ft
	 WHERE ft.Id = f.FileTypeId
	 ) AS FileType
	 ,f.CreatedBy
	 ,f.DateCreated

FROM @ReturnedValues as ReturnedValues
	INNER JOIN dbo.Files as f
		ON f.Id = ReturnedValues.ReturnedId

END