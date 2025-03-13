USE [master]
GO
      
CREATE LOGIN [NT AUTHORITY\NETWORK SERVICE] FROM WINDOWS 
CREATE LOGIN [NT AUTHORITY\SYSTEM] FROM WINDOWS 
GO      
      
USE [Entrvo]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[01_Batchs]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[01_Batchs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Time] [datetime2](7) NOT NULL,
	[FileName] [nvarchar](128) NULL,
 CONSTRAINT [PK_01_Batchs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[04_Batchs]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[04_Batchs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Time] [datetime2](7) NOT NULL,
	[FileName] [nvarchar](128) NULL,
 CONSTRAINT [PK_04_Batchs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AccessLogs]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccessLogs](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Time] [datetimeoffset](7) NOT NULL,
	[ApplicationName] [nvarchar](48) NOT NULL,
	[Method] [nvarchar](24) NOT NULL,
	[RequestUri] [nvarchar](max) NOT NULL,
	[Origin] [nvarchar](64) NULL,
	[User] [nvarchar](48) NULL,
	[StatusCode] [int] NOT NULL,
	[Elapsed] [int] NOT NULL,
 CONSTRAINT [PK_AccessLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [uniqueidentifier] NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserType] [int] NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [uniqueidentifier] NOT NULL,
	[RoleId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [uniqueidentifier] NOT NULL,
	[FirstName] [nvarchar](36) NULL,
	[LastName] [nvarchar](36) NULL,
	[LastTimeSignedIn] [datetimeoffset](7) NULL,
	[SecurityStamp] [nvarchar](max) NOT NULL,
	[ConcurrencyStamp] [nvarchar](max) NOT NULL,
	[UserType] [int] NOT NULL,
	[IsActive] [bit] NULL,
	[Comment] [nvarchar](256) NULL,
	[HomePhoneNumber] [nvarchar](36) NULL,
	[WorkPhoneNumber] [nvarchar](36) NULL,
	[TimeRegistered] [datetimeoffset](7) NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [uniqueidentifier] NOT NULL,
	[LoginProvider] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Batchs]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Batchs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Time] [datetime2](7) NOT NULL,
	[FileName] [nvarchar](128) NOT NULL,
	[NewEndValidityDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Batchs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Cards]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cards](
	[ClientRef] [nvarchar](32) NOT NULL,
	[Amount] [decimal](7, 2) NOT NULL,
	[ValidUntil] [smalldatetime] NULL,
	[ContractId] [nvarchar](64) NULL,
	[ConsumerId] [nvarchar](64) NULL,
	[Status] [int] NOT NULL,
	[TimeModified] [datetime2](7) NOT NULL,
	[Version] [int] NOT NULL,
	[timestamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_Cards] PRIMARY KEY CLUSTERED 
(
	[ClientRef] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Consumers]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Consumers](
	[Id] [uniqueidentifier] NOT NULL,
	[CardNumber] [nvarchar](32) NULL,
	[ClientRef] [nvarchar](32) NULL,
	[FirstName] [nvarchar](32) NULL,
	[LastName] [nvarchar](32) NULL,
	[LPN1] [nvarchar](24) NULL,
	[LPN2] [nvarchar](24) NULL,
	[LPN3] [nvarchar](24) NULL,
	[Memo1] [nvarchar](64) NULL,
	[Memo2] [nvarchar](64) NULL,
	[Memo3] [nvarchar](64) NULL,
	[ValidUntil] [datetime2](7) NULL,
	[Amount] [decimal](7, 2) NULL,
	[ContractId] [nvarchar](64) NULL,
	[ConsumerId] [nvarchar](64) NULL,
	[IsActive] [bit] NOT NULL,
	[timestamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_Consumers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomerNotes]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerNotes](
	[NoteUID] [int] NOT NULL,
	[SourceObjectUID] [int] NOT NULL,
 CONSTRAINT [PK_CustomerNotes] PRIMARY KEY CLUSTERED 
(
	[NoteUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Customers]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customers](
	[CustomerUID] [int] NOT NULL,
	[AccountBalance] [decimal](7, 2) NOT NULL,
	[AllotmentGroup] [nvarchar](48) NULL,
	[BalanceDue] [decimal](7, 2) NOT NULL,
	[Classification] [nvarchar](48) NULL,
	[CustomerType] [nvarchar](48) NULL,
	[DisallowChecks] [bit] NOT NULL,
	[EmployeeID] [nvarchar](48) NULL,
	[FirstName] [nvarchar](48) NULL,
	[GroupName] [nvarchar](64) NULL,
	[HasLetter] [bit] NOT NULL,
	[HasNote] [bit] NOT NULL,
	[HasPendingLetter] [bit] NOT NULL,
	[HomePhone] [nvarchar](48) NULL,
	[ImportedName] [nvarchar](48) NULL,
	[LastName] [nvarchar](48) NULL,
	[MiddleName] [nvarchar](48) NULL,
	[ModifyDate] [datetime2](7) NULL,
	[NamePrefix] [nvarchar](48) NULL,
	[NameSuffix] [nvarchar](48) NULL,
	[NonEmployeeID] [nvarchar](48) NULL,
	[OtherPhone] [nvarchar](48) NULL,
	[PrimaryAddress] [nvarchar](48) NULL,
	[PrimaryEmail] [int] NULL,
	[PrimaryFinancialAccount] [int] NOT NULL,
	[ScofflawFlag] [bit] NOT NULL,
	[Subclassification] [nvarchar](48) NULL,
	[TertiaryID] [nvarchar](48) NULL,
	[WorkPhone] [nvarchar](48) NULL,
	[timestamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED 
(
	[CustomerUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DbAuditLogs]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DbAuditLogs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[Type] [nvarchar](max) NOT NULL,
	[TableName] [nvarchar](128) NULL,
	[DateTime] [datetimeoffset](7) NOT NULL,
	[OldValues] [nvarchar](max) NULL,
	[NewValues] [nvarchar](max) NULL,
	[AffectedColumns] [nvarchar](max) NULL,
	[PrimaryKey] [nvarchar](max) NULL,
 CONSTRAINT [PK_DbAuditLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Emails]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Emails](
	[EmailAddressUID] [int] NOT NULL,
	[CustomerUID] [int] NOT NULL,
	[EffectiveRank] [int] NOT NULL,
	[EmailAddress] [nvarchar](64) NULL,
	[EndDate] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[IsHistorical] [bit] NOT NULL,
	[ModifyDate] [datetime2](7) NULL,
	[Priority] [int] NOT NULL,
	[SourceType] [int] NOT NULL,
	[StartDate] [datetime2](7) NULL,
	[Type] [nvarchar](20) NULL,
	[timestamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_Emails] PRIMARY KEY CLUSTERED 
(
	[EmailAddressUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Events]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Events](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Time] [datetime2](7) NOT NULL,
	[Type] [int] NOT NULL,
	[Message] [nvarchar](256) NULL,
	[Details] [nvarchar](max) NULL,
	[FileUrl] [nvarchar](256) NULL,
	[ConsumerId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notes]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notes](
	[NoteUID] [int] NOT NULL,
	[Document] [nvarchar](20) NULL,
	[EndDate] [datetime2](7) NULL,
	[IsHistorical] [bit] NOT NULL,
	[ModifyDate] [datetime2](7) NULL,
	[NoteText] [nvarchar](max) NULL,
	[NoteType] [nvarchar](20) NULL,
	[TableUIDofSourceObject] [int] NOT NULL,
	[timestamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_Notes] PRIMARY KEY CLUSTERED 
(
	[NoteUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PermitNotes]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PermitNotes](
	[NoteUID] [int] NOT NULL,
	[SourceObjectUID] [int] NOT NULL,
 CONSTRAINT [PK_PermitNotes] PRIMARY KEY CLUSTERED 
(
	[NoteUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Permits]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Permits](
	[PermitUID] [int] NOT NULL,
	[AccessGroup] [nvarchar](48) NULL,
	[ActiveCredentialID] [nvarchar](48) NULL,
	[AddedValue] [int] NOT NULL,
	[AllocateReturnDate] [datetime2](7) NULL,
	[AllocateStartDate] [datetime2](7) NULL,
	[Allocator] [int] NULL,
	[AnonymousUserID] [nvarchar](48) NULL,
	[BulkPermit] [nvarchar](48) NULL,
	[CardMode] [nvarchar](48) NULL,
	[Comment] [nvarchar](256) NULL,
	[ControlGroup] [int] NULL,
	[Current] [bit] NOT NULL,
	[Custody] [nvarchar](48) NULL,
	[CustodyType] [nvarchar](48) NULL,
	[DeactivateReason] [nvarchar](48) NULL,
	[DeactivatedDate] [datetime2](7) NULL,
	[Deallocator] [nvarchar](48) NULL,
	[DepositAmountPaid] [decimal](7, 2) NULL,
	[DepositFee] [decimal](7, 2) NULL,
	[Drawer] [nvarchar](48) NULL,
	[EffectiveDate] [datetime2](7) NULL,
	[EmailNotificationAddress] [nvarchar](48) NULL,
	[ExpirationDate] [datetime2](7) NULL,
	[ExporttoWPSRequired] [bit] NOT NULL,
	[FeeSchedAtPurch] [int] NULL,
	[HasDepositbeenrefunded] [bit] NOT NULL,
	[HasLetter] [bit] NOT NULL,
	[HasNote] [bit] NOT NULL,
	[HasPendingLetter] [bit] NOT NULL,
	[IsDeactivated] [bit] NOT NULL,
	[IsDestroyed] [bit] NOT NULL,
	[IsEmailNotifyReq] [bit] NOT NULL,
	[IsFulfilling] [bit] NOT NULL,
	[IsHistorical] [bit] NOT NULL,
	[IsMailingReq] [bit] NOT NULL,
	[IsMissing] [bit] NOT NULL,
	[IsMissingfromCustody] [bit] NOT NULL,
	[IsPermitDirectFulfilled] [bit] NOT NULL,
	[IsPossConfReq] [bit] NOT NULL,
	[IsReturned] [bit] NOT NULL,
	[IsReturningtoMainInventory] [bit] NOT NULL,
	[IsTerminated] [bit] NOT NULL,
	[IssueNumber] [int] NOT NULL,
	[LaneControllerStampDate] [datetime2](7) NULL,
	[LastExportedtoWPS] [datetime2](7) NULL,
	[MailTrackingNumber] [nvarchar](48) NULL,
	[MailingAddress] [nvarchar](48) NULL,
	[MailingDate] [datetime2](7) NULL,
	[MaximumValue] [nvarchar](48) NULL,
	[MissingDate] [datetime2](7) NULL,
	[MissingReason] [nvarchar](48) NULL,
	[ModifyDate] [datetime2](7) NULL,
	[PemissionRawNumber] [int] NOT NULL,
	[PendingExpirationDate] [datetime2](7) NULL,
	[PermitAllotment] [nvarchar](48) NULL,
	[PermitAmountDue] [decimal](7, 2) NOT NULL,
	[PermitNumber] [nvarchar](48) NULL,
	[PermitNumberRange] [nvarchar](48) NULL,
	[PermitSeriesType] [nvarchar](48) NULL,
	[PermitDirectStatus] [nvarchar](48) NULL,
	[PhysicalGroupType] [nvarchar](48) NULL,
	[PossessionDate] [datetime2](7) NULL,
	[PurchasingCustomer] [int] NULL,
	[PurchasingThirdParty] [nvarchar](48) NULL,
	[RenewalGracePeriodEndingDate] [datetime2](7) NULL,
	[RenewalStatus] [nvarchar](48) NULL,
	[RenewalUID] [int] NULL,
	[ReplacementPermit] [nvarchar](48) NULL,
	[ReservationNoShowCutoffDate] [datetime2](7) NULL,
	[ReserveEndDate] [datetime2](7) NULL,
	[ReserveHold] [nvarchar](48) NULL,
	[ReserveStartDate] [datetime2](7) NULL,
	[Reserved] [nvarchar](48) NULL,
	[ReturnDate] [nvarchar](48) NULL,
	[ReturnReason] [nvarchar](48) NULL,
	[ShippingMethod] [nvarchar](48) NULL,
	[SoldDate] [datetime2](7) NULL,
	[StallId] [nvarchar](48) NULL,
	[StallType] [nvarchar](48) NULL,
	[Status] [nvarchar](48) NULL,
	[TerminatedDate] [datetime2](7) NULL,
	[WorkflowStatus] [nvarchar](48) NULL,
	[timestamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_Permits] PRIMARY KEY CLUSTERED 
(
	[PermitUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Settings]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Settings](
	[Key] [nvarchar](64) NOT NULL,
	[Value] [nvarchar](256) NOT NULL,
	[RowVersion] [timestamp] NULL,
 CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED 
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Vehicles]    Script Date: 3/13/2025 8:51:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vehicles](
	[VehicleUID] [int] NOT NULL,
	[AmountDue] [decimal](7, 2) NOT NULL,
	[HasHandheldNotifications] [bit] NOT NULL,
	[LicensePlate] [nvarchar](20) NULL,
	[ModifyDate] [datetime2](7) NULL,
	[OfficialRegistrationinformationfromDMV] [nvarchar](20) NULL,
	[PlateRegExpMonth] [nvarchar](20) NULL,
	[PlateRegExpYear] [nvarchar](20) NULL,
	[PlateSeries] [nvarchar](20) NULL,
	[PlateType] [nvarchar](20) NULL,
	[Province] [nvarchar](20) NULL,
	[RoVREligible] [bit] NOT NULL,
	[RoVRLastSent] [datetime2](7) NULL,
	[RoVRLastUpdated] [datetime2](7) NULL,
	[ScofflawFlag] [bit] NOT NULL,
	[SelectedforNextRoVR] [bit] NOT NULL,
	[SeriesEndDate] [int] NOT NULL,
	[SeriesStartDate] [int] NOT NULL,
	[VehicleColor] [nvarchar](20) NULL,
	[VehicleMake] [nvarchar](20) NULL,
	[VehicleModel] [nvarchar](20) NULL,
	[VehicleStyle] [nvarchar](20) NULL,
	[Vin] [nvarchar](20) NULL,
	[Year] [nvarchar](20) NULL,
	[timestamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_Vehicles] PRIMARY KEY CLUSTERED 
(
	[VehicleUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT (sysdatetimeoffset()) FOR [TimeRegistered]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[CustomerNotes]  WITH CHECK ADD  CONSTRAINT [FK_CustomerNotes_Customers_SourceObjectUID] FOREIGN KEY([SourceObjectUID])
REFERENCES [dbo].[Customers] ([CustomerUID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CustomerNotes] CHECK CONSTRAINT [FK_CustomerNotes_Customers_SourceObjectUID]
GO
ALTER TABLE [dbo].[CustomerNotes]  WITH CHECK ADD  CONSTRAINT [FK_CustomerNotes_Notes_NoteUID] FOREIGN KEY([NoteUID])
REFERENCES [dbo].[Notes] ([NoteUID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CustomerNotes] CHECK CONSTRAINT [FK_CustomerNotes_Notes_NoteUID]
GO
ALTER TABLE [dbo].[Emails]  WITH CHECK ADD  CONSTRAINT [FK_Emails_Customers_CustomerUID] FOREIGN KEY([CustomerUID])
REFERENCES [dbo].[Customers] ([CustomerUID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Emails] CHECK CONSTRAINT [FK_Emails_Customers_CustomerUID]
GO
ALTER TABLE [dbo].[PermitNotes]  WITH CHECK ADD  CONSTRAINT [FK_PermitNotes_Notes_NoteUID] FOREIGN KEY([NoteUID])
REFERENCES [dbo].[Notes] ([NoteUID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PermitNotes] CHECK CONSTRAINT [FK_PermitNotes_Notes_NoteUID]
GO
ALTER TABLE [dbo].[PermitNotes]  WITH CHECK ADD  CONSTRAINT [FK_PermitNotes_Permits_SourceObjectUID] FOREIGN KEY([SourceObjectUID])
REFERENCES [dbo].[Permits] ([PermitUID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PermitNotes] CHECK CONSTRAINT [FK_PermitNotes_Permits_SourceObjectUID]
GO
ALTER TABLE [dbo].[Permits]  WITH CHECK ADD  CONSTRAINT [FK_Permits_Customers_PurchasingCustomer] FOREIGN KEY([PurchasingCustomer])
REFERENCES [dbo].[Customers] ([CustomerUID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Permits] CHECK CONSTRAINT [FK_Permits_Customers_PurchasingCustomer]
GO
