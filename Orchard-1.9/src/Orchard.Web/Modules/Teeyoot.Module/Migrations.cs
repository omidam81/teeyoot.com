using System;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Localization.Records;
using Orchard.Users.Models;
using Teeyoot.Module.Models;
using Orchard.ContentManagement;
using Orchard;
using System.Reflection;
using System.Data;
using NHibernate;
using System.Collections.Generic;
using Teeyoot.Module.Common.Enums;
using Teeyoot.Module.Services.Interfaces;

namespace Teeyoot.Module
{
    public class Migrations : DataMigrationImpl
    {
        private readonly IRepository<CommonSettingsRecord> _commonSettingsRepository;
        private readonly IRepository<TeeyootUserPartRecord> _teeyootUserPartRepository;
        private readonly IRepository<CountryRecord> _countryRepository;
        private readonly IRepository<CampaignRecord> _campaignRepository;
        private readonly IRepository<UserPartRecord> _userPartRepository;
        private readonly IRepository<OrderRecord> _orderRepository;
        private readonly IRepository<CultureRecord> _cultureRepository;
        private readonly IContentManager _contentManager;
        private readonly IRepository<CampaignDesign> _campaignDesign;
        private readonly IOrchardServices _orchardServices;
        private readonly ISessionLocator _sessionLocator;
        private readonly ICampaignService _campaignservice;

        private readonly IRepository<CampaignProductRecord> _campaignproducts;

        public Migrations(
            IRepository<CommonSettingsRecord> commonSettingsRepository,
            IRepository<TeeyootUserPartRecord> teeyootUserPartRepository,
            IRepository<CountryRecord> countryRepository,
            IRepository<CampaignRecord> campaignRepository,
            IRepository<UserPartRecord> userPartRepository,
            IRepository<OrderRecord> orderRepository,
            IRepository<CultureRecord> cultureRepository,
            IContentManager contentManager,
            IRepository<CampaignDesign> campaignDesign,
            IOrchardServices orchardServices,
            ISessionLocator sessionLocator,
            IRepository<CampaignProductRecord> campaignProductRecord,
            ICampaignService campaignservice
            )
        {
            _commonSettingsRepository = commonSettingsRepository;
            _teeyootUserPartRepository = teeyootUserPartRepository;
            _countryRepository = countryRepository;
            _campaignRepository = campaignRepository;
            _userPartRepository = userPartRepository;
            _orderRepository = orderRepository;
            _cultureRepository = cultureRepository;
            _contentManager = contentManager;
            _campaignDesign = campaignDesign;
            _orchardServices = orchardServices;
            _sessionLocator = sessionLocator;
            _campaignproducts = campaignProductRecord;
            _campaignservice = campaignservice;
        }

        public int Create()
        {
            SchemaBuilder.CreateTable(typeof(ProductImageRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("Width")
                    .Column<int>("Height")
                    .Column<int>("Ppi")
                    .Column<int>("PrintableFrontLeft")
                    .Column<int>("PrintableFrontTop")
                    .Column<int>("PrintableFrontWidth")
                    .Column<int>("PrintableFrontHeight")
                    .Column<int>("ChestLineFront")
                    .Column<int>("PrintableBackLeft")
                    .Column<int>("PrintableBackTop")
                    .Column<int>("PrintableBackWidth")
                    .Column<int>("PrintableBackHeight")
                    .Column<int>("ChestLineBack")
                    .Column<string>("Gender", c => c.Nullable())

                );

            SchemaBuilder.CreateTable(typeof(ProductGroupRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name", c => c.WithLength(150))
                );

            SchemaBuilder.CreateTable(typeof(ProductHeadlineRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name", c => c.WithLength(150))
                );

            SchemaBuilder.CreateTable(typeof(ProductRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name", c => c.WithLength(250))
                    .Column<int>("ProductHeadlineRecord_Id")
                    .Column<int>("ProductImageRecord_Id")
                    .Column<string>("Materials", c => c.Nullable())
                    .Column<string>("Details", c => c.Unlimited().Nullable())
                );

            SchemaBuilder.CreateTable(typeof(ProductColorRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name", c => c.WithLength(150))
                    .Column<string>("Value", c => c.WithLength(10))
                    .Column<int>("Importance", c => c.Nullable())
                );

            SchemaBuilder.CreateTable(typeof(SizeCodeRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name", c => c.WithLength(10))
                );

            SchemaBuilder.CreateTable(typeof(ProductSizeRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("SizeCodeRecord_Id")
                    .Column<double>("LengthCm")
                    .Column<double>("WidthCm")
                    .Column<double>("SleeveCm", c => c.Nullable())
                    .Column<double>("LengthInch")
                    .Column<double>("WidthInch")
                    .Column<double>("SleeveInch", c => c.Nullable())
                );

            SchemaBuilder.CreateTable(typeof(LinkProductColorRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("ProductColorRecord_Id")
                    .Column<int>("ProductRecord_Id")
                    .Column<double>("BaseCost")
                );

            SchemaBuilder.CreateTable(typeof(LinkProductSizeRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("ProductSizeRecord_Id")
                    .Column<int>("ProductRecord_Id")
                );

            SchemaBuilder.CreateTable(typeof(TeeyootUserPartRecord).Name,
                table => table
                    .ContentPartRecord()
                    .Column<string>("PublicName", c => c.WithLength(50))
                    .Column<string>("PhoneNumber", c => c.WithLength(50))
                    .Column<DateTime>("CreatedUtc")
                );

            SchemaBuilder.CreateTable(typeof(CurrencyRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Code", c => c.WithLength(10))
                    .Column<string>("Name", c => c.WithLength(150))
                    .Column<string>("ShortName", c => c.WithLength(50))
                );

            SchemaBuilder.CreateTable(typeof(CampaignRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Title")
                    .Column<int>("TeeyootUserId")
                    .Column<int>("ProductCountGoal")
                    .Column<int>("ProductCountSold")
                    .Column<string>("Design", c => c.Unlimited())
                    .Column<string>("Description", c => c.Unlimited())
                    .Column<DateTime>("EndDate")
                    .Column<string>("URL")
                    .Column<bool>("IsForCharity", c => c.WithDefault(false))
                    .Column<bool>("BackSideByDefault", c => c.WithDefault(false))
                );

            SchemaBuilder.CreateTable(typeof(CampaignProductRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("CampaignRecord_Id")
                    .Column<int>("CurrencyRecord_Id")
                    .Column<double>("Price")
                    .Column<double>("BaseCost")
                    .Column<int>("ProductRecord_Id")
                    .Column<int>("ProductColorRecord_Id")
                );

            SchemaBuilder.CreateTable(typeof(FontRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Family", c => c.WithLength(150))
                    .Column<string>("FileName", c => c.WithLength(150))
                    .Column<string>("Tags", c => c.WithLength(250))
                    .Column<int>("Priority", c => c.Nullable())
                );

            SchemaBuilder.CreateTable(typeof(SwatchRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name", c => c.WithLength(150))
                    .Column<bool>("InStock", c => c.WithDefault(true))
                    .Column<int>("Red")
                    .Column<int>("Blue")
                    .Column<int>("Green")
                );

            SchemaBuilder.CreateTable(typeof(CampaignCategoriesRecord).Name,
                table => table
                    .ContentPartRecord()
                    .Column<string>("Name", c => c.WithLength(50))
                );

            ContentDefinitionManager.AlterPartDefinition(typeof(CampaignCategoriesPart).Name,
                part => part.Attachable(false));
            ContentDefinitionManager.AlterTypeDefinition("CampaignCategories", type => type
                .WithPart(typeof(CampaignCategoriesPart).Name)
                .WithPart("CommonPart")
                );

            SchemaBuilder.CreateForeignKey("Product_ProductHeadline", "ProductRecord",
                new[] { "ProductHeadlineRecord_Id" }, "ProductHeadlineRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("Product_ProductImage", "ProductRecord", new[] { "ProductImageRecord_Id" },
                "ProductImageRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("ProductSize_SizeCode", "ProductSizeRecord", new[] { "SizeCodeRecord_Id" },
                "SizeCodeRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkProductColor_Product", "LinkProductColorRecord",
                new[] { "ProductRecord_Id" }, "ProductRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkProductColor_ProductColor", "LinkProductColorRecord",
                new[] { "ProductColorRecord_Id" }, "ProductColorRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkProductSize_Product", "LinkProductSizeRecord",
                new[] { "ProductRecord_Id" }, "ProductRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkProductSize_ProductSize", "LinkProductSizeRecord",
                new[] { "ProductSizeRecord_Id" }, "ProductSizeRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("CampaignProduct_Campaign", "CampaignProductRecord",
                new[] { "CampaignRecord_Id" }, "CampaignRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("CampaignProduct_Product", "CampaignProductRecord",
                new[] { "ProductRecord_Id" }, "ProductRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("CampaignProduct_ProductColor", "CampaignProductRecord",
                new[] { "ProductColorRecord_Id" }, "ProductColorRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("Campaign_TeeyootUser", "CampaignRecord", new[] { "TeeyootUserId" },
                "TeeyootUserPartRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("CampaignProduct_Currency", "CampaignProductRecord",
                new[] { "CurrencyRecord_Id" }, "CurrencyRecord", new[] { "Id" });


            ContentDefinitionManager.AlterPartDefinition(typeof(TeeyootUserPart).Name, part => part
                .Attachable(false)
                );

            ContentDefinitionManager.AlterTypeDefinition("TeeyootUser", type => type
                .WithPart(typeof(TeeyootUserPart).Name)
                .WithPart("UserPart")
                );

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table
                    .AddColumn<string>("Alias", c => c.WithLength(100))
                );

            SchemaBuilder.CreateTable(typeof(OrderRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Email", c => c.WithLength(100))
                    .Column<string>("FirstName", c => c.WithLength(100))
                    .Column<string>("LastName", c => c.WithLength(100))
                    .Column<string>("City", c => c.WithLength(100))
                    .Column<string>("State", c => c.WithLength(100))
                    .Column<string>("Country", c => c.WithLength(100))
                    .Column<double>("TotalPrice")
                    .Column<int>("CurrencyRecord_Id")
                );

            SchemaBuilder.CreateTable(typeof(LinkOrderCampaignProductRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("OrderRecord_Id")
                    .Column<int>("CampaignProductRecord_Id")
                    .Column<int>("Count")
                    .Column<string>("Size", c => c.WithLength(10)));

            SchemaBuilder.CreateForeignKey("Order_Currency", "OrderRecord", new[] { "CurrencyRecord_Id" },
                "CurrencyRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkOrderCampaignProduct_Order", "LinkOrderCampaignProductRecord",
                new[] { "OrderRecord_Id" }, "OrderRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkOrderCampaignProduct_CampaignProduct", "LinkOrderCampaignProductRecord",
                new[] { "CampaignProductRecord_Id" }, "CampaignProductRecord", new[] { "Id" });

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<string>("Tags", c => c.Unlimited()));

            SchemaBuilder.CreateTable(typeof(LinkProductGroupRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("ProductGroupRecord_Id")
                    .Column<int>("ProductRecord_Id")
                );

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<string>("StreetAddress", c => c.Unlimited()));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<string>("PostalCode", c => c.WithLength(50)));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<string>("PhoneNumber", c => c.WithLength(50)));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.DropColumn("StreetAddress"));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.DropColumn("PostalCode"));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.DropColumn("PhoneNumber"));
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<string>("StreetAddress", c => c.Unlimited()));
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<string>("PostalCode", c => c.WithLength(50)));
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<string>("PhoneNumber", c => c.WithLength(50)));

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<DateTime>("Created", c => c.NotNull()));
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<DateTime>("Paid", c => c.Nullable()));

            SchemaBuilder.CreateTable(typeof(CampaignStatusRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name", c => c.WithLength(150))
                );

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<int>("CampaignStatusRecord_Id"));

            SchemaBuilder.CreateForeignKey("Campaign_Status", "CampaignRecord", new[] { "CampaignStatusRecord_Id" },
                "CampaignStatusRecord", new[] { "Id" });

            SchemaBuilder.AlterTable(typeof(LinkOrderCampaignProductRecord).Name,
                table => table.DropColumn("Size"));

            SchemaBuilder.AlterTable(typeof(LinkOrderCampaignProductRecord).Name,
                table => table.AddColumn<int>("SizeId"));

            SchemaBuilder.CreateForeignKey("OrderProduct_Size", "LinkOrderCampaignProductRecord", new[] { "SizeId" },
                "ProductSizeRecord", new[] { "Id" });

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<DateTime>("StartDate"));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.DropColumn("Tags"));

            SchemaBuilder.AlterTable(typeof(CampaignCategoriesRecord).Name, table => table.AddColumn<bool>("IsVisible"));

            SchemaBuilder.CreateTable(typeof(LinkCampaignAndCategoriesRecord).Name, table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("CampaignRecord_Id")
                .Column<int>("CampaignCategoriesPartRecord_Id")
                );

            SchemaBuilder.CreateForeignKey("LinkCampaignAndCategories_Campaign", "LinkCampaignAndCategoriesRecord",
                new[] { "CampaignRecord_Id" }, "CampaignRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkCampaignAndCategories_CampaignCategories",
                "LinkCampaignAndCategoriesRecord", new[] { "CampaignCategoriesPartRecord_Id" },
                "CampaignCategoriesPartRecord", new[] { "Id" });

            SchemaBuilder.DropForeignKey("LinkOrderCampaignProductRecord", "OrderProduct_Size");

            SchemaBuilder.AlterTable(typeof(LinkOrderCampaignProductRecord).Name,
                table => table.DropColumn("SizeId"));

            SchemaBuilder.AlterTable(typeof(LinkOrderCampaignProductRecord).Name,
                table => table.AddColumn<int>("ProductSizeRecord_Id"));

            SchemaBuilder.CreateForeignKey("OrderProduct_Size", "LinkOrderCampaignProductRecord",
                new[] { "ProductSizeRecord_Id" }, "ProductSizeRecord", new[] { "Id" });

            SchemaBuilder.CreateTable(typeof(StoreRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("TeeyootUserId")
                    .Column<string>("Title")
                    .Column<string>("Description")
                    .Column<bool>("HideStore", c => c.WithDefault(false))
                    .Column<bool>("CrossSelling", c => c.WithDefault(false))
                );

            SchemaBuilder.CreateTable(typeof(LinkStoreCampaignRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("StoreRecord_Id")
                    .Column<int>("CampaignRecord_Id")
                );

            SchemaBuilder.CreateForeignKey("Store_TeeyootUser", "StoreRecord", new[] { "TeeyootUserId" },
                "TeeyootUserPartRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkStoreCampaignRecord_CampaignRecord", "LinkStoreCampaignRecord",
                new[] { "CampaignRecord_Id" }, "CampaignRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkOrderCampaignProduct_StoreRecord", "LinkStoreCampaignRecord",
                new[] { "StoreRecord_Id" }, "StoreRecord", new[] { "Id" });

            SchemaBuilder.DropForeignKey("LinkCampaignAndCategoriesRecord",
                "LinkCampaignAndCategories_CampaignCategories");
            SchemaBuilder.DropTable(typeof(CampaignCategoriesRecord).Name);
            ContentDefinitionManager.DeletePartDefinition(typeof(CampaignCategoriesPart).Name);
            ContentDefinitionManager.DeleteTypeDefinition("CampaignCategories");

            SchemaBuilder.CreateTable(typeof(CampaignCategoriesRecord).Name, table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Name", column => column.WithLength(50))
                .Column<bool>("IsVisible")
                );

            SchemaBuilder.CreateForeignKey("LinkCampaignAndCategories_CampaignCategories",
                "LinkCampaignAndCategoriesRecord", new[] { "CampaignCategoriesPartRecord_Id" }, "CampaignCategoriesRecord",
                new[] { "Id" });

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<bool>("IsFeatured", cl => cl.WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(StoreRecord).Name,
                table => table.AddColumn<string>("Url", cl => cl.WithLength(150)));

            SchemaBuilder.CreateTable(typeof(TShirtCostRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<float>("FirstScreenCost")
                    .Column<float>("AdditionalScreenCosts")
                    .Column<float>("InkCost")
                    .Column<int>("PrintsPerLitre")
                    .Column<float>("LabourCost")
                    .Column<int>("LabourTimePerColourPerPrint")
                    .Column<int>("LabourTimePerSidePrintedPerPrint")
                    .Column<float>("CostOfMaterial")
                    .Column<float>("PercentageMarkUpRequired")
                    .Column<float>("DTGPrintPrice")
                );

            SchemaBuilder.CreateTable(typeof(OrderStatusRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name", c => c.WithLength(150))
                );

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<DateTime>("Reserved", c => c.Nullable()));
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name, table => table.AddColumn<int>("OrderStatusRecord_Id"));

            SchemaBuilder.CreateForeignKey("Order_Status", "OrderRecord", new[] { "OrderStatusRecord_Id" },
                "OrderStatusRecord", new[] { "Id" });

            SchemaBuilder.CreateTable(typeof(PayoutRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<DateTime>("Date")
                    .Column<string>("Event", c => c.Unlimited())
                    .Column<double>("Amount")
                    .Column<bool>("IsPlus")
                    .Column<int>("UserId")
                );

            SchemaBuilder.CreateForeignKey("PayoutRecord_UserId", "PayoutRecord", new[] { "UserId" },
                "TeeyootUserPartRecord", new[] { "Id" });

            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("Street", c => c.Unlimited()));
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("Suit", c => c.WithLength(50)));
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("City", c => c.WithLength(100)));
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("State", c => c.WithLength(100)));
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("Zip", c => c.WithLength(50)));

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<string>("OrderPublicId", c => c.NotNull().WithLength(50)));

            SchemaBuilder.AlterTable(typeof(TShirtCostRecord).Name, table => table.DropColumn("CostOfMaterial"));

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<bool>("IsActive", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(PayoutRecord).Name,
                table => table.AddColumn<string>("Status", c => c.NotNull().WithLength(50)));

            SchemaBuilder.CreateTable(typeof(PromotionRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<DateTime>("Expiration")
                    .Column<string>("PromoId", c => c.WithLength(15))
                    .Column<string>("AmountType", c => c.WithLength(50))
                    .Column<int>("AmountSize")
                    .Column<string>("DiscountType", c => c.WithLength(50))
                    .Column<bool>("Status")
                    .Column<int>("Redeemed")
                );

            SchemaBuilder.AlterTable(typeof(PromotionRecord).Name,
                table => table.AddColumn<int>("Userid", c => c.NotNull()));

            SchemaBuilder.CreateForeignKey("PromotionRecord_UserId", "PromotionRecord", new[] { "UserId" },
                "TeeyootUserPartRecord", new[] { "Id" });

            SchemaBuilder.CreateTable(typeof(OrderHistoryRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<DateTime>("EventDate")
                    .Column<string>("Event", c => c.Unlimited())
                    .Column<int>("OrderRecord_Id")
                );

            SchemaBuilder.CreateForeignKey("OrderHistory_Order", "OrderHistoryRecord", new[] { "OrderRecord_Id" },
                "OrderRecord", new[] { "Id" });

            SchemaBuilder.CreateTable(typeof(PaymentInformationRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("AccountNumber")
                    .Column<string>("BankName")
                    .Column<int>("ContactNumber")
                    .Column<string>("MessAdmin")
                    .Column<int>("TranzactionId")
                );

            SchemaBuilder.CreateForeignKey("Tranzaction_Payout_Id", "PaymentInformationRecord", new[] { "TranzactionId" },
                "PayoutRecord", new[] { "Id" });

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name, table => table.AddColumn<double>("Promotion"));

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name, table => table.AddColumn<double>("TotalPriceWithPromo"));

            SchemaBuilder.AlterTable(typeof(PaymentInformationRecord).Name,
                table => table.AddColumn<string>("AccountHolderName"));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<bool>("IsActive", c => c.NotNull().WithDefault(true)));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<bool>("IsApproved", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<string>("TranzactionId", c => c.Nullable()));

            SchemaBuilder.CreateTable(typeof(MessageRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("UserId")
                    .Column<string>("Text")
                    .Column<string>("From")
                    .Column<DateTime>("SendDate")
                );

            SchemaBuilder.CreateForeignKey("Message_User_Id", "MessageRecord", new[] { "UserId" }, "TeeyootUserPartRecord",
                new[] { "Id" });

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<DateTime>("Delete", c => c.WithDefault(null)));

            SchemaBuilder.AlterTable(typeof(MessageRecord).Name, table => table.DropColumn("Text"));

            SchemaBuilder.AlterTable(typeof(MessageRecord).Name,
                table => table.AddColumn<string>("Text", c => c.Unlimited()));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.DropColumn("Delete"));

            SchemaBuilder.AlterTable(typeof(MessageRecord).Name, table => table.DropColumn("From"));

            SchemaBuilder.AlterTable(typeof(MessageRecord).Name,
                table => table.AddColumn<string>("Sender", c => c.WithLength(50)));

            SchemaBuilder.AlterTable(typeof(MessageRecord).Name, table => table.AddColumn<int>("CampaignId"));

            SchemaBuilder.CreateForeignKey("Message_Campaign_Id", "MessageRecord", new[] { "CampaignId" },
                "CampaignRecord", new[] { "Id" });

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<DateTime>("WhenDeleted", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<bool>("IsPrivate", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(MessageRecord).Name,
                table => table.AddColumn<bool>("IsApprowed", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(MessageRecord).Name,
                table => table.AddColumn<string>("Subject", c => c.WithLength(50)));

            SchemaBuilder.AlterTable(typeof(LinkProductSizeRecord).Name,
                table => table.AddColumn<float>("SizeCost", c => c.NotNull().WithDefault(float.Parse("0"))));

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<bool>("ProfitPaid", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.CreateTable(typeof(MailChimpSettingsPartRecord).Name,
                table => table
                    .ContentPartRecord()
                    .Column<string>("ApiKey", c => c.WithLength(50))
                    .Column<string>("Culture", c => c.WithLength(50)));


            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<string>("CampaignProfit", c => c.NotNull().WithDefault(string.Empty)));

            SchemaBuilder.AlterTable(typeof(TShirtCostRecord).Name,
                table => table.AddColumn<int>("SalesGoal", c => c.NotNull().WithDefault(500)));

            SchemaBuilder.AlterTable(typeof(TShirtCostRecord).Name,
                table => table.AddColumn<int>("MaxColors", c => c.NotNull().WithDefault(10)));

            SchemaBuilder.CreateTable(typeof(CommonSettingsRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name", c => c.WithLength(50))
                    .Column<bool>("Value"));

            SchemaBuilder.DropTable(typeof(CommonSettingsRecord).Name);

            SchemaBuilder.CreateTable(typeof(CommonSettingsRecord).Name, table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<bool>("DoNotAcceptAnyNewCampaigns", column => column.NotNull().WithDefault(false))
                .Column<int>("ColoursPerPrint", column => column.NotNull().WithDefault(0)));

            var commonSettings = new CommonSettingsRecord();
            _commonSettingsRepository.Create(commonSettings);


            SchemaBuilder.CreateTable(typeof(PaymentSettingsRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Culture", c => c.WithLength(50))
                    .Column<int>("PaymentMethod"));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<int>("Environment", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("PublicKey", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("PrivateKey", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("MerchantId", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("ClientToken", c => c.Nullable()));



            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<int>("ProductMinimumGoal", c => c.NotNull().WithDefault(0)));

            SchemaBuilder.AlterTable(typeof(ProductRecord).Name,
                table => table.AddColumn<DateTime>("WhenDeleted", c => c.Nullable()));


            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name, table => table.DropColumn("ClientToken"));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("ClientToken", c => c.Unlimited()));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<bool>("Rejected", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<DateTime>("WhenApproved", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<DateTime>("WhenSentOut", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name, table => table.DropColumn("WhenApproved"));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<DateTime>("WhenApproved", c => c.Nullable()));

            SchemaBuilder.CreateTable(typeof(CheckoutCampaignRequest).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<DateTime>("RequestUtcDate", column => column.NotNull())
                    .Column<bool>("EmailSent", column => column.NotNull().WithDefault(false))
                    .Column<DateTime>("EmailSentUtcDate"));

            SchemaBuilder.AlterTable(typeof(CheckoutCampaignRequest).Name,
                table => table
                    .AddColumn<string>("Email", column => column.WithLength(100).NotNull()));

            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table
                    .DropColumn("ColoursPerPrint"));

            SchemaBuilder.CreateTable(typeof(BringBackCampaignRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("CampaignRecord_Id")
                    .Column<string>("Email", c => c.NotNull())
                );

            SchemaBuilder.CreateForeignKey("BringBackCampaign_Order", "BringBackCampaignRecord",
                new[] { "CampaignRecord_Id" }, "CampaignRecord", new[] { "Id" });

            SchemaBuilder.CreateTable(typeof(ArtRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name")
                    .Column<string>("FileName")
                );

            SchemaBuilder.AlterTable(typeof(CheckoutCampaignRequest).Name,
                table => table
                    .DropColumn("EmailSent"));



            SchemaBuilder.AlterTable(typeof(PayoutRecord).Name,
                table => table
                    .AddColumn<int>("Currency_Id", column => column.NotNull().WithDefault(1)));

            SchemaBuilder.CreateForeignKey("CurrencyKey", "PayoutRecord", new[] { "Currency_Id" },
                "CurrencyRecord", new[] { "Id" });


            SchemaBuilder.AlterTable(typeof(PaymentInformationRecord).Name,
                table => table
                    .DropColumn("AccountNumber"));

            SchemaBuilder.AlterTable(typeof(PaymentInformationRecord).Name,
                table => table
                    .DropColumn("ContactNumber"));

            SchemaBuilder.AlterTable(typeof(PaymentInformationRecord).Name,
                table => table
                    .AddColumn<string>("AccountNumber", column => column.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentInformationRecord).Name,
                table => table
                    .AddColumn<string>("ContactNumber", column => column.Nullable()));


            SchemaBuilder.AlterTable(typeof(PromotionRecord).Name,
                table => table
                    .DropColumn("AmountSize"));

            SchemaBuilder.AlterTable(typeof(PromotionRecord).Name,
                table => table
                    .AddColumn<double>("AmountSize"));

            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("Country", c => c.WithLength(100)));
            SchemaBuilder.AlterTable(typeof(LinkProductColorRecord).Name,
                table => table
                    .DropColumn("BaseCost"));

            SchemaBuilder.AlterTable(typeof(ProductRecord).Name,
                table => table
                    .AddColumn<float>("BaseCost"));

            SchemaBuilder.AlterTable(typeof(CampaignProductRecord).Name,
                table => table.AddColumn<int>("SecondProductColorRecord_Id", c => c.Nullable()));
            SchemaBuilder.AlterTable(typeof(CampaignProductRecord).Name,
                table => table.AddColumn<int>("ThirdProductColorRecord_Id", c => c.Nullable()));
            SchemaBuilder.AlterTable(typeof(CampaignProductRecord).Name,
                table => table.AddColumn<int>("FourthProductColorRecord_Id", c => c.Nullable()));
            SchemaBuilder.AlterTable(typeof(CampaignProductRecord).Name,
                table => table.AddColumn<int>("FifthProductColorRecord_Id", c => c.Nullable()));

            SchemaBuilder.CreateForeignKey("CampaignProduct_ProductColorSecond", "CampaignProductRecord",
                new[] { "SecondProductColorRecord_Id" }, "ProductColorRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("CampaignProduct_ProductColorThird", "CampaignProductRecord",
                new[] { "ThirdProductColorRecord_Id" }, "ProductColorRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("CampaignProduct_ProductColorFourth", "CampaignProductRecord",
                new[] { "FourthProductColorRecord_Id" }, "ProductColorRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("CampaignProduct_ProductColorFifth", "CampaignProductRecord",
                new[] { "FifthProductColorRecord_Id" }, "ProductColorRecord", new[] { "Id" });

            SchemaBuilder.AlterTable(typeof(LinkOrderCampaignProductRecord).Name,
                table => table.AddColumn<int>("ProductColorRecord_Id", c => c.Nullable()));

            SchemaBuilder.CreateForeignKey("LinkOrderCampaignProduct_ProductColor", "LinkOrderCampaignProductRecord",
                new[] { "ProductColorRecord_Id" }, "ProductColorRecord", new[] { "Id" });

            SchemaBuilder.CreateTable(typeof(DeliverySettingRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("State")
                    .Column<double>("DeliveryCost")
                );


            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name, table => table.DropColumn("PaymentMethod"));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<bool>("CashDeliv", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<bool>("PayPal", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<bool>("Mol", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<bool>("CreditCard", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("MerchantIdMol", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("VerifyKey", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<bool>("IsArchived", c => c.NotNull().WithDefault(false)));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<int>("BaseCampaignId", c => c.Nullable()));

            SchemaBuilder.CreateForeignKey("CampaignRecord_BaseCampaignId", "CampaignRecord",
                new[] { "BaseCampaignId" }, "CampaignRecord", new[] { "Id" });


            SchemaBuilder.AlterTable(typeof(CampaignProductRecord).Name,
                table => table.AddColumn<DateTime>("WhenDeleted", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<int>("CntFrontColor"));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<int>("CntBackColor"));

            SchemaBuilder.AlterTable(typeof(DeliverySettingRecord).Name,
                table => table.AddColumn<bool>("Enabled", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table =>
                    table.AddColumn<string>("CampaignCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            SchemaBuilder.AlterTable(typeof(CurrencyRecord).Name,
                table =>
                    table.AddColumn<string>("CurrencyCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            SchemaBuilder.AlterTable(typeof(TShirtCostRecord).Name,
                table => table.AddColumn<string>("CostCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(CampaignCategoriesRecord).Name,
                table =>
                    table.AddColumn<string>("CategoriesCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table.AddColumn<string>("CashOnDeliveryAvailabilityMessage"));

            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table =>
                    table.AddColumn<string>("TeeyootUserCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table.DropColumn("CashOnDeliveryAvailabilityMessage"));

            SchemaBuilder.AlterTable(typeof(ArtRecord).Name,
                table => table.AddColumn<string>("ArtCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(ProductImageRecord).Name,
                table => table.AddColumn<string>("ProdImgCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(ProductHeadlineRecord).Name,
                table =>
                    table.AddColumn<string>("ProdHeadCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(ProductSizeRecord).Name,
                table =>
                    table.AddColumn<string>("ProdSizeCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(ProductGroupRecord).Name,
                table =>
                    table.AddColumn<string>("ProdGroupCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(FontRecord).Name,
                table => table.AddColumn<string>("FontCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            SchemaBuilder.AlterTable(typeof(ProductColorRecord).Name,
                table =>
                    table.AddColumn<string>("ProdColorCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(SwatchRecord).Name,
                table => table.AddColumn<string>("SwatchCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table.AddColumn<string>("CommonCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            SchemaBuilder.AlterTable(typeof(DeliverySettingRecord).Name,
                table =>
                    table.AddColumn<string>("DeliveryCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<double>("Delivery"));

            SchemaBuilder.CreateTable(typeof(MailTemplateSubjectRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("TemplateName")
                    .Column<string>("Culture")
                    .Column<string>("Subject")
                );

            // Migration #95.

            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table.AddColumn<string>("CashOnDeliveryAvailabilityMessage"));
            //
            // Tab names for payment methods
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("CashDelivTabName"));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("PayPalTabName"));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("MolTabName"));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("CreditCardTabName"));
            // Notes for payment methods
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("CashDelivNote"));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("PayPalNote"));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("MolNote"));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("CreditCardNote"));
            //
            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table.AddColumn<string>("CheckoutPageRightSideContent", c => c.Unlimited()));

            // Migration #96.

            SchemaBuilder.AlterTable(typeof(CurrencyRecord).Name,
                table => table.AddColumn<string>("FlagFileName", c => c.WithLength(1024)));

            SchemaBuilder.CreateTable(typeof(CountryRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Code", c => c.WithLength(10))
                    .Column<string>("Name", c => c.WithLength(150))
                );

            SchemaBuilder.CreateTable(typeof(LinkCountryCurrencyRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("CurrencyRecord_Id")
                    .Column<int>("CountryRecord_Id")
                );

            SchemaBuilder.CreateTable(typeof(LinkCountryCultureRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("CultureRecord_Id")
                    .Column<int>("CountryRecord_Id")
                );

            SchemaBuilder.CreateForeignKey("LinkCountryCurrency_Currency", "LinkCountryCurrencyRecord",
                new[] { "CurrencyRecord_Id" }, "CountryRecord", new[] { "Id" });

            SchemaBuilder.CreateForeignKey("LinkCountryCurrency_Country", "LinkCountryCurrencyRecord",
                new[] { "CountryRecord_Id" }, "CountryRecord", new[] { "Id" });

            SchemaBuilder.CreateForeignKey("LinkCountryCulture_Culture", "LinkCountryCultureRecord",
                new[] { "CultureRecord_Id" }, "CountryRecord", new[] { "Id" });

            SchemaBuilder.CreateForeignKey("LinkCountryCulture_Country", "LinkCountryCultureRecord",
                new[] { "CountryRecord_Id" }, "CountryRecord", new[] { "Id" });

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<int>("CountryRecord_Id", c => c.WithDefault(1)));

            SchemaBuilder.CreateForeignKey("Campaign_Currency", "CampaignRecord",
                new[] { "CountryRecord_Id" }, "CountryRecord", new[] { "Id" });

            //TODO: (auth:keinlekan) Удалить колонку после того, как заработает полностью новая логика по привязке к странам
            //SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.DropColumn("CampaignCulture"));

            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table.AddColumn<int>("CountryRecord_Id", c => c.WithDefault(1)));

            SchemaBuilder.CreateForeignKey("CommonSettings_Currency", "CommonSettingsRecord",
                new[] { "CountryRecord_Id" }, "CountryRecord", new[] { "Id" });

            //TODO: (auth:keinlekan) Удалить колонку после того, как заработает полностью новая логика по привязке к странам
            //SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name, table => table.DropColumn("CommonCulture"));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<int>("CountryRecord_Id", c => c.WithDefault(1)));

            SchemaBuilder.CreateForeignKey("PaymentSettings_Currency", "PaymentSettingsRecord",
                new[] { "CountryRecord_Id" }, "CountryRecord", new[] { "Id" });

            //TODO: (auth:keinlekan) Удалить колонку после того, как заработает полностью новая логика по привязке к странам
            //SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name, table => table.DropColumn("CommonCulture"));

            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<int>("CurrencyRecord_Id"));

            SchemaBuilder.CreateForeignKey("TeeyootUserPartRecord_CurrencyRecord", "TeeyootUserPartRecord",
                new[] { "CurrencyRecord_Id" }, "CurrencyRecord", new[] { "Id" });

            SchemaBuilder.AlterTable(typeof(PromotionRecord).Name,
                table => table.AddColumn<DateTime>("Created", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PromotionRecord).Name,
                table => table.AddColumn<int>("CampaignId", c => c.Nullable()));


            SchemaBuilder.AlterTable(typeof(CampaignCategoriesRecord).Name,
                table => table.AddColumn<int>("CountryRecord_Id", c => c.WithDefault(1)));

            SchemaBuilder.CreateForeignKey("CampaignCategories_Currency", "CampaignCategoriesRecord",
                new[] { "CountryRecord_Id" }, "CountryRecord", new[] { "Id" });

            //TODO: (auth:keinlekan) Удалить колонку после того, как заработает полностью новая логика по привязке к странам
            //SchemaBuilder.AlterTable(typeof(CampaignCategoriesRecord).Name, table => table.DropColumn("CountryRecord_Id"));

            SchemaBuilder.AlterTable(typeof(CurrencyRecord).Name,
                table => table.AddColumn<double>("PriceBuyers", c => c.WithDefault(1)));

            SchemaBuilder.AlterTable(typeof(CurrencyRecord).Name,
                table => table.AddColumn<double>("PriceSellers", c => c.WithDefault(1)));

            SchemaBuilder.AlterTable(typeof(CurrencyRecord).Name,
                table => table.AddColumn<bool>("IsConvert", c => c.WithDefault(false)));

            ContentDefinitionManager.AlterPartDefinition(
                "AllCountryWidgetPart",
                builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition(
                "AllCountryWidget",
                cfg => cfg
                    .WithPart("AllCountryWidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );

            // Default is used only to set existing records Country field to 1 before applying notnull constaraint.
            SchemaBuilder.AlterTable(typeof(DeliverySettingRecord).Name,
                table => table.AddColumn<int>("Country_id", c => c.WithDefault(1).NotNull()));
            SchemaBuilder.CreateForeignKey("DeliverySettings_Country",
                typeof(DeliverySettingRecord).Name, new[] { "Country_id" },
                typeof(CountryRecord).Name, new[] { "Id" });

            SchemaBuilder.AlterTable(typeof(DeliverySettingRecord).Name,
                table => table.AddColumn<double>("PostageCost"));

            SchemaBuilder.AlterTable(typeof(DeliverySettingRecord).Name,
                table => table.AddColumn<double>("CodCost"));

            // Dropping the default constarint for Country field
            SchemaBuilder.ExecuteSql(
                @"declare @table_name nvarchar(256)
                    declare @col_name nvarchar(256)
                    declare @Command  nvarchar(1000)

                    set @table_name = N'Teeyoot_Module_DeliverySettingRecord'
                    set @col_name = N'Country_id'

                    select @Command = 'ALTER TABLE ' + @table_name + ' drop constraint ' + d.name
                     from sys.tables t   
                      join    sys.default_constraints d       
                       on d.parent_object_id = t.object_id  
                      join    sys.columns c      
                       on c.object_id = t.object_id      
                        and c.column_id = d.parent_column_id
                     where t.name = @table_name
                      and c.name = @col_name

                    execute (@Command)"
                );

            SchemaBuilder.CreateTable(typeof(DeliveryInternationalSettingRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("CountryFrom_Id", c => c.NotNull())
                    .Column<int>("CountryTo_Id", c => c.NotNull())
                );

            SchemaBuilder.CreateForeignKey("FK_DeliveryInternationalSetting_CountryFrom",
                typeof(DeliveryInternationalSettingRecord).Name, new[] { "CountryFrom_Id" },
                typeof(CountryRecord).Name, new[] { "Id" });

            SchemaBuilder.CreateForeignKey("FK_DeliveryInternationalSetting_CountryTo",
                typeof(DeliveryInternationalSettingRecord).Name, new[] { "CountryTo_Id" },
                typeof(CountryRecord).Name, new[] { "Id" });

            SchemaBuilder.AlterTable(typeof(PayoutRecord).Name,
                table => table.AddColumn<bool>("IsProfitPaid", c => c.WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(PayoutRecord).Name,
                table => table.AddColumn<bool>("IsCampiaign", c => c.WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(PayoutRecord).Name,
                table => table.AddColumn<bool>("IsOrder", c => c.WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<int>("CurrencyRecord_Id"));

            SchemaBuilder.CreateForeignKey("FK_Campaign_Currency",
                typeof(CampaignRecord).Name, new[] { "CurrencyRecord_Id" },
                typeof(CurrencyRecord).Name, new[] { "Id" });

            SchemaBuilder.ExecuteSql(@"
                update 
                [dbo].[Teeyoot_Module_CampaignRecord]
                set CurrencyRecord_Id = 1
                ");

            SchemaBuilder.AlterTable(typeof(PromotionRecord).Name,
                table => table.AddColumn<int>("CurrencyRecord_Id"));

            SchemaBuilder.CreateForeignKey("Promotion_Currency",
                "PromotionRecord", new[] { "CurrencyRecord_Id" },
                "CurrencyRecord", new[] { "Id" });

            return 113;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table
                    .AddColumn<string>("Alias", c => c.WithLength(100))
                );

            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.CreateTable(typeof(OrderRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Email", c => c.WithLength(100))
                    .Column<string>("FirstName", c => c.WithLength(100))
                    .Column<string>("LastName", c => c.WithLength(100))
                    .Column<string>("City", c => c.WithLength(100))
                    .Column<string>("State", c => c.WithLength(100))
                    .Column<string>("Country", c => c.WithLength(100))
                    .Column<double>("TotalPrice")
                    .Column<int>("CurrencyRecord_Id")
                );

            SchemaBuilder.CreateTable(typeof(LinkOrderCampaignProductRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("OrderRecord_Id")
                    .Column<int>("CampaignProductRecord_Id")
                    .Column<int>("Count")
                    .Column<string>("Size", c => c.WithLength(10)));

            SchemaBuilder.CreateForeignKey("Order_Currency", "OrderRecord", new[] { "CurrencyRecord_Id" },
                "CurrencyRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkOrderCampaignProduct_Order", "LinkOrderCampaignProductRecord",
                new[] { "OrderRecord_Id" }, "OrderRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkOrderCampaignProduct_CampaignProduct", "LinkOrderCampaignProductRecord",
                new[] { "CampaignProductRecord_Id" }, "CampaignProductRecord", new[] { "Id" });

            return 4;
        }

        public int UpdateFrom5()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<string>("Tags", c => c.Unlimited()));

            return 6;
        }

        public int UpdateFrom6()
        {
            SchemaBuilder.DropForeignKey("ProductRecord", "Product_ProductGroup");

            SchemaBuilder.AlterTable(typeof(ProductRecord).Name, table => table.DropColumn("ProductGroupRecord_Id"));

            SchemaBuilder.CreateTable(typeof(LinkProductGroupRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("ProductGroupRecord_Id")
                    .Column<int>("ProductRecord_Id")
                );

            SchemaBuilder.CreateForeignKey("LinkProductGroup_Product", "LinkProductGroupRecord",
                new[] { "ProductRecord_Id" }, "ProductRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkProductGroup_ProductGroup", "LinkProductGroupRecord",
                new[] { "ProductGroupRecord_Id" }, "ProductGroupRecord", new[] { "Id" });

            return 7;
        }

        public int UpdateFrom7()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<string>("StreetAddress", c => c.Unlimited()));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<string>("PostalCode", c => c.WithLength(50)));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<string>("PhoneNumber", c => c.WithLength(50)));

            return 8;
        }

        public int UpdateFrom8()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.DropColumn("StreetAddress"));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.DropColumn("PostalCode"));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.DropColumn("PhoneNumber"));
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<string>("StreetAddress", c => c.Unlimited()));
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<string>("PostalCode", c => c.WithLength(50)));
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<string>("PhoneNumber", c => c.WithLength(50)));


            return 9;
        }

        public int UpdateFrom9()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<DateTime>("Created", c => c.NotNull()));
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<DateTime>("Paid", c => c.Nullable()));

            SchemaBuilder.CreateTable(typeof(CampaignStatusRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name", c => c.WithLength(150))
                );

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<int>("CampaignStatusRecord_Id"));

            SchemaBuilder.CreateForeignKey("Campaign_Status", "CampaignRecord", new[] { "CampaignStatusRecord_Id" },
                "CampaignStatusRecord", new[] { "Id" });

            return 10;
        }

        public int UpdateFrom10()
        {
            SchemaBuilder.AlterTable(typeof(LinkOrderCampaignProductRecord).Name,
                table => table.DropColumn("Size"));

            SchemaBuilder.AlterTable(typeof(LinkOrderCampaignProductRecord).Name,
                table => table.AddColumn<int>("SizeId"));

            return 11;
        }

        public int UpdateFrom11()
        {
            SchemaBuilder.CreateForeignKey("OrderProduct_Size", "LinkOrderCampaignProductRecord", new[] { "SizeId" },
                "ProductSizeRecord", new[] { "Id" });

            return 12;
        }

        public int UpdateFrom12()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<DateTime>("StartDate"));

            return 13;
        }

        public int UpdateFrom13()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.DropColumn("Tags"));

            SchemaBuilder.AlterTable(typeof(CampaignCategoriesRecord).Name, table => table.AddColumn<bool>("IsVisible"));

            SchemaBuilder.CreateTable(typeof(LinkCampaignAndCategoriesRecord).Name, table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("CampaignRecord_Id")
                .Column<int>("CampaignCategoriesPartRecord_Id")
                );

            SchemaBuilder.CreateForeignKey("LinkCampaignAndCategories_Campaign", "LinkCampaignAndCategoriesRecord",
                new[] { "CampaignRecord_Id" }, "CampaignRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkCampaignAndCategories_CampaignCategories",
                "LinkCampaignAndCategoriesRecord", new[] { "CampaignCategoriesPartRecord_Id" },
                "CampaignCategoriesPartRecord", new[] { "Id" });

            return 14;
        }

        public int UpdateFrom14()
        {
            SchemaBuilder.DropForeignKey("LinkOrderCampaignProductRecord", "OrderProduct_Size");

            SchemaBuilder.AlterTable(typeof(LinkOrderCampaignProductRecord).Name,
                table => table.DropColumn("SizeId"));

            SchemaBuilder.AlterTable(typeof(LinkOrderCampaignProductRecord).Name,
                table => table.AddColumn<int>("ProductSizeRecord_Id"));

            SchemaBuilder.CreateForeignKey("OrderProduct_Size", "LinkOrderCampaignProductRecord",
                new[] { "ProductSizeRecord_Id" }, "ProductSizeRecord", new[] { "Id" });

            return 15;
        }

        public int UpdateFrom15()
        {
            SchemaBuilder.CreateTable(typeof(StoreRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("TeeyootUserId")
                    .Column<string>("Title")
                    .Column<string>("Description")
                    .Column<bool>("HideStore", c => c.WithDefault(false))
                    .Column<bool>("CrossSelling", c => c.WithDefault(false))
                );

            SchemaBuilder.CreateTable(typeof(LinkStoreCampaignRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("StoreRecord_Id")
                    .Column<int>("CampaignRecord_Id")
                );

            SchemaBuilder.CreateForeignKey("Store_TeeyootUser", "StoreRecord", new[] { "TeeyootUserId" },
                "TeeyootUserPartRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkStoreCampaignRecord_CampaignRecord", "LinkStoreCampaignRecord",
                new[] { "CampaignRecord_Id" }, "CampaignRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("LinkOrderCampaignProduct_StoreRecord", "LinkStoreCampaignRecord",
                new[] { "StoreRecord_Id" }, "StoreRecord", new[] { "Id" });

            return 16;
        }

        public int UpdateFrom16()
        {
            SchemaBuilder.DropForeignKey("LinkCampaignAndCategoriesRecord",
                "LinkCampaignAndCategories_CampaignCategories");

            SchemaBuilder.DropTable(typeof(CampaignCategoriesRecord).Name);
            ContentDefinitionManager.DeletePartDefinition(typeof(CampaignCategoriesPart).Name);
            ContentDefinitionManager.DeleteTypeDefinition("CampaignCategories");

            SchemaBuilder.CreateTable(typeof(CampaignCategoriesRecord).Name, table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Name", column => column.WithLength(50))
                .Column<bool>("IsVisible")
                );

            return 17;
        }

        public int UpdateFrom17()
        {
            SchemaBuilder.CreateForeignKey("LinkCampaignAndCategories_CampaignCategories",
                "LinkCampaignAndCategoriesRecord", new[] { "CampaignCategoriesPartRecord_Id" }, "CampaignCategoriesRecord",
                new[] { "Id" });

            return 18;
        }

        public int UpdateFrom18()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<bool>("IsFeatured", cl => cl.WithDefault(false)));

            return 19;
        }

        public int UpdateFrom19()
        {
            SchemaBuilder.AlterTable(typeof(StoreRecord).Name,
                table => table.AddColumn<string>("Url", cl => cl.WithLength(150)));

            return 20;
        }

        public int UpdateFrom20()
        {
            SchemaBuilder.CreateTable(typeof(TShirtCostRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<float>("FirstScreenCost")
                    .Column<float>("AdditionalScreenCosts")
                    .Column<float>("InkCost")
                    .Column<int>("PrintsPerLitre")
                    .Column<float>("LabourCost")
                    .Column<int>("LabourTimePerColourPerPrint")
                    .Column<int>("LabourTimePerSidePrintedPerPrint")
                    .Column<float>("CostOfMaterial")
                    .Column<float>("PercentageMarkUpRequired")
                    .Column<float>("DTGPrintPrice")
                );

            return 21;
        }

        public int UpdateFrom21()
        {
            SchemaBuilder.CreateTable(typeof(OrderStatusRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name", c => c.WithLength(150))
                );

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<DateTime>("Reserved", c => c.Nullable()));
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name, table => table.AddColumn<int>("OrderStatusRecord_Id"));

            SchemaBuilder.CreateForeignKey("Order_Status", "OrderRecord", new[] { "OrderStatusRecord_Id" },
                "OrderStatusRecord", new[] { "Id" });

            return 22;
        }

        public int UpdateFrom22()
        {
            SchemaBuilder.CreateTable(typeof(PayoutRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<DateTime>("Date")
                    .Column<string>("Event", c => c.Unlimited())
                    .Column<double>("Amount")
                    .Column<bool>("IsPlus")
                    .Column<int>("UserId")
                );

            SchemaBuilder.CreateForeignKey("PayoutRecord_UserId", "PayoutRecord", new[] { "UserId" },
                "TeeyootUserPartRecord", new[] { "Id" });

            return 23;
        }

        public int UpdateFrom23()
        {
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("Street", c => c.Unlimited()));
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("Suit", c => c.WithLength(50)));
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("City", c => c.WithLength(100)));
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("State", c => c.WithLength(100)));
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("Zip", c => c.WithLength(50)));

            return 24;
        }

        public int UpdateFrom24()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<string>("OrderPublicId", c => c.NotNull().WithLength(50)));
            return 25;
        }

        public int UpdateFrom25()
        {
            SchemaBuilder.AlterTable(typeof(TShirtCostRecord).Name, table => table.DropColumn("CostOfMaterial"));
            return 26;
        }

        public int UpdateFrom26()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<bool>("IsActive", c => c.NotNull().WithDefault(false)));
            return 27;
        }

        public int UpdateFrom27()
        {
            SchemaBuilder.AlterTable(typeof(PayoutRecord).Name,
                table => table.AddColumn<string>("Status", c => c.NotNull().WithLength(50)));
            return 28;
        }

        public int UpdateFrom28()
        {
            SchemaBuilder.CreateTable(typeof(PromotionRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<DateTime>("Expiration")
                    .Column<string>("PromoId", c => c.WithLength(15))
                    .Column<string>("AmountType", c => c.WithLength(50))
                    .Column<int>("AmountSize")
                    .Column<string>("DiscountType", c => c.WithLength(50))
                    .Column<bool>("Status")
                    .Column<int>("Redeemed")
                );
            return 29;
        }

        public int UpdateFrom29()
        {
            SchemaBuilder.AlterTable(typeof(PromotionRecord).Name,
                table => table.AddColumn<int>("Userid", c => c.NotNull()));

            SchemaBuilder.CreateForeignKey("PromotionRecord_UserId", "PromotionRecord", new[] { "UserId" },
                "TeeyootUserPartRecord", new[] { "Id" });

            return 30;
        }

        public int UpdateFrom30()
        {
            SchemaBuilder.CreateTable(typeof(OrderHistoryRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<DateTime>("EventDate")
                    .Column<string>("Event", c => c.Unlimited())
                    .Column<int>("OrderRecord_Id")
                );

            SchemaBuilder.CreateForeignKey("OrderHistory_Order", "OrderHistoryRecord", new[] { "OrderRecord_Id" },
                "OrderRecord", new[] { "Id" });

            return 31;
        }

        public int UpdateFrom31()
        {
            SchemaBuilder.CreateTable(typeof(PaymentInformationRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("AccountNumber")
                    .Column<string>("BankName")
                    .Column<int>("ContactNumber")
                    .Column<string>("MessAdmin")
                    .Column<int>("TranzactionId")
                );

            SchemaBuilder.CreateForeignKey("Tranzaction_Payout_Id", "PaymentInformationRecord", new[] { "TranzactionId" },
                "PayoutRecord", new[] { "Id" });

            return 32;
        }

        public int UpdateFrom32()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name, table => table.AddColumn<double>("Promotion"));

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name, table => table.AddColumn<double>("TotalPriceWithPromo"));

            return 33;
        }

        public int UpdateFrom33()
        {
            SchemaBuilder.AlterTable(typeof(PaymentInformationRecord).Name,
                table => table.AddColumn<string>("AccountHolderName"));
            return 34;
        }

        public int UpdateFrom34()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<bool>("IsActive", c => c.NotNull().WithDefault(true)));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<bool>("IsApproved", c => c.NotNull().WithDefault(false)));

            return 35;
        }

        public int UpdateFrom35()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<string>("TranzactionId", c => c.Nullable()));
            return 36;
        }

        public int UpdateFrom36()
        {
            SchemaBuilder.CreateTable(typeof(MessageRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("UserId")
                    .Column<string>("Text")
                    .Column<string>("From")
                    .Column<DateTime>("SendDate")
                );

            SchemaBuilder.CreateForeignKey("Message_User_Id", "MessageRecord", new[] { "UserId" }, "TeeyootUserPartRecord",
                new[] { "Id" });

            return 37;
        }

        public int UpdateFrom37()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<DateTime>("Delete", c => c.WithDefault(null)));
            return 38;
        }

        public int UpdateFrom38()
        {
            SchemaBuilder.AlterTable(typeof(MessageRecord).Name, table => table.DropColumn("Text"));

            SchemaBuilder.AlterTable(typeof(MessageRecord).Name,
                table => table.AddColumn<string>("Text", c => c.Unlimited()));

            return 39;
        }

        public int UpdateFrom39()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.DropColumn("Delete"));

            return 40;
        }

        public int UpdateFrom40()
        {
            SchemaBuilder.AlterTable(typeof(MessageRecord).Name, table => table.DropColumn("From"));

            SchemaBuilder.AlterTable(typeof(MessageRecord).Name,
                table => table.AddColumn<string>("Sender", c => c.WithLength(50)));

            return 41;
        }

        public int UpdateFrom41()
        {

            SchemaBuilder.AlterTable(typeof(MessageRecord).Name, table => table.AddColumn<int>("CampaignId"));

            SchemaBuilder.CreateForeignKey("Message_Campaign_Id", "MessageRecord", new[] { "CampaignId" },
                "CampaignRecord", new[] { "Id" });

            return 42;
        }

        public int UpdateFrom42()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<DateTime>("WhenDeleted", c => c.Nullable()));

            return 43;
        }

        public int UpdateFrom43()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<bool>("IsPrivate", c => c.NotNull().WithDefault(false)));

            return 44;
        }

        public int UpdateFrom44()
        {
            SchemaBuilder.AlterTable(typeof(MessageRecord).Name,
                table => table.AddColumn<bool>("IsApprowed", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(MessageRecord).Name,
                table => table.AddColumn<string>("Subject", c => c.WithLength(50)));

            return 45;
        }

        public int UpdateFrom45()
        {
            SchemaBuilder.AlterTable(typeof(LinkProductSizeRecord).Name,
                table => table.AddColumn<float>("SizeCost", c => c.NotNull().WithDefault(float.Parse("0"))));

            return 46;
        }

        public int UpdateFrom46()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<bool>("ProfitPaid", c => c.NotNull().WithDefault(false)));

            return 47;
        }

        public int UpdateFrom47()
        {
            SchemaBuilder.CreateTable(typeof(MailChimpSettingsPartRecord).Name,
                table => table
                    .ContentPartRecord()
                    .Column<string>("ApiKey", c => c.WithLength(50))
                    .Column<string>("Culture", c => c.WithLength(50)));

            return 48;
        }

        public int UpdateFrom48()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<string>("CampaignProfit", c => c.NotNull().WithDefault(string.Empty)));

            return 49;
        }

        public int UpdateFrom49()
        {
            SchemaBuilder.AlterTable(typeof(TShirtCostRecord).Name,
                table => table.AddColumn<int>("SalesGoal", c => c.NotNull().WithDefault(500)));

            return 50;
        }

        public int UpdateFrom50()
        {
            SchemaBuilder.AlterTable(typeof(TShirtCostRecord).Name,
                table => table.AddColumn<int>("MaxColors", c => c.NotNull().WithDefault(10)));

            return 51;
        }

        public int UpdateFrom51()
        {
            SchemaBuilder.CreateTable(typeof(CommonSettingsRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name", c => c.WithLength(50))
                    .Column<bool>("Value"));

            return 52;
        }

        public int UpdateFrom52()
        {
            SchemaBuilder.DropTable(typeof(CommonSettingsRecord).Name);

            SchemaBuilder.CreateTable(typeof(CommonSettingsRecord).Name, table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<bool>("DoNotAcceptAnyNewCampaigns", column => column.NotNull().WithDefault(false))
                .Column<int>("ColoursPerPrint", column => column.NotNull().WithDefault(0)));

            var commonSettings = new CommonSettingsRecord();
            _commonSettingsRepository.Create(commonSettings);

            return 53;
        }

        public int UpdateFrom53()
        {
            SchemaBuilder.CreateTable(typeof(PaymentSettingsRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Culture", c => c.WithLength(50))
                    .Column<int>("PaymentMethod"));

            return 54;
        }

        public int UpdateFrom54()
        {
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<int>("Environment", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("PublicKey", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("PrivateKey", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("MerchantId", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("ClientToken", c => c.Nullable()));

            return 55;
        }

        public int UpdateFrom55()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<int>("ProductMinimumGoal", c => c.NotNull().WithDefault(0)));

            return 56;
        }

        public int UpdateFrom56()
        {
            SchemaBuilder.AlterTable(typeof(ProductRecord).Name,
                table => table.AddColumn<DateTime>("WhenDeleted", c => c.Nullable()));
            return 57;
        }

        public int UpdateFrom57()
        {
            //SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name, table => table.AlterColumn("ClientToken", c => c.Unlimited()));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name, table => table.DropColumn("ClientToken"));
            return 58;
        }

        public int UpdateFrom58()
        {
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("ClientToken", c => c.Unlimited()));
            return 59;
        }

        public int UpdateFrom59()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<bool>("Rejected", c => c.NotNull().WithDefault(false)));

            return 60;
        }


        public int UpdateFrom60()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<DateTime>("WhenApproved", c => c.Nullable()));
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<DateTime>("WhenSentOut", c => c.Nullable()));

            return 61;
        }

        public int UpdateFrom61()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name, table => table.DropColumn("WhenApproved"));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<DateTime>("WhenApproved", c => c.Nullable()));

            return 62;
        }

        public int UpdateFrom62()
        {
            SchemaBuilder.CreateTable(typeof(CheckoutCampaignRequest).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<DateTime>("RequestUtcDate", column => column.NotNull())
                    .Column<bool>("EmailSent", column => column.NotNull().WithDefault(false))
                    .Column<DateTime>("EmailSentUtcDate"));

            return 63;
        }

        public int UpdateFrom63()
        {
            SchemaBuilder.AlterTable(typeof(CheckoutCampaignRequest).Name,
                table => table
                    .AddColumn<string>("Email", column => column.WithLength(100)));

            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table
                    .DropColumn("ColoursPerPrint"));

            return 64;
        }

        public int UpdateFrom64()
        {
            SchemaBuilder.CreateTable(typeof(BringBackCampaignRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("CampaignRecord_Id")
                    .Column<string>("Email", c => c.NotNull())
                );

            SchemaBuilder.CreateForeignKey("BringBackCampaign_Order", "BringBackCampaignRecord",
                new[] { "CampaignRecord_Id" }, "CampaignRecord", new[] { "Id" });

            return 65;
        }

        public int UpdateFrom65()
        {
            SchemaBuilder.CreateTable(typeof(ArtRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name")
                    .Column<string>("FileName")
                );

            return 66;
        }

        public int UpdateFrom66()
        {
            SchemaBuilder.AlterTable(typeof(CheckoutCampaignRequest).Name,
                table => table
                    .DropColumn("EmailSent"));

            return 67;
        }

        public int UpdateFrom67()
        {
            SchemaBuilder.AlterTable(typeof(PayoutRecord).Name,
                table => table
                    .AddColumn<int>("Currency_Id", column => column.NotNull().WithDefault(1)));

            SchemaBuilder.CreateForeignKey("CurrencyKey", "PayoutRecord", new[] { "Currency_Id" },
                "CurrencyRecord", new[] { "Id" });
            return 68;
        }

        public int UpdateFrom68()
        {
            SchemaBuilder.AlterTable(typeof(PaymentInformationRecord).Name,
                table => table
                    .DropColumn("AccountNumber"));

            SchemaBuilder.AlterTable(typeof(PaymentInformationRecord).Name,
                table => table
                    .DropColumn("ContactNumber"));

            return 69;
        }


        public int UpdateFrom69()
        {
            SchemaBuilder.AlterTable(typeof(PaymentInformationRecord).Name,
                table => table
                    .AddColumn<string>("AccountNumber", column => column.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentInformationRecord).Name,
                table => table
                    .AddColumn<string>("ContactNumber", column => column.Nullable()));

            return 70;
        }

        public int UpdateFrom70()
        {
            SchemaBuilder.AlterTable(typeof(PromotionRecord).Name,
                table => table
                    .DropColumn("AmountSize"));

            SchemaBuilder.AlterTable(typeof(PromotionRecord).Name,
                table => table
                    .AddColumn<double>("AmountSize"));

            return 71;
        }

        public int UpdateFrom71()
        {
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("Country", c => c.WithLength(100)));
            return 72;
        }

        public int UpdateFrom72()
        {
            SchemaBuilder.AlterTable(typeof(LinkProductColorRecord).Name,
                table => table
                    .DropColumn("BaseCost"));

            SchemaBuilder.AlterTable(typeof(ProductRecord).Name,
                table => table
                    .AddColumn<float>("BaseCost"));

            return 73;
        }

        public int UpdateFrom73()
        {
            SchemaBuilder.AlterTable(typeof(CampaignProductRecord).Name,
                table => table.AddColumn<int>("SecondProductColorRecord_Id", c => c.Nullable()));
            SchemaBuilder.AlterTable(typeof(CampaignProductRecord).Name,
                table => table.AddColumn<int>("ThirdProductColorRecord_Id", c => c.Nullable()));
            SchemaBuilder.AlterTable(typeof(CampaignProductRecord).Name,
                table => table.AddColumn<int>("FourthProductColorRecord_Id", c => c.Nullable()));
            SchemaBuilder.AlterTable(typeof(CampaignProductRecord).Name,
                table => table.AddColumn<int>("FifthProductColorRecord_Id", c => c.Nullable()));

            SchemaBuilder.CreateForeignKey("CampaignProduct_ProductColorSecond", "CampaignProductRecord",
                new[] { "SecondProductColorRecord_Id" }, "ProductColorRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("CampaignProduct_ProductColorThird", "CampaignProductRecord",
                new[] { "ThirdProductColorRecord_Id" }, "ProductColorRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("CampaignProduct_ProductColorFourth", "CampaignProductRecord",
                new[] { "FourthProductColorRecord_Id" }, "ProductColorRecord", new[] { "Id" });
            SchemaBuilder.CreateForeignKey("CampaignProduct_ProductColorFifth", "CampaignProductRecord",
                new[] { "FifthProductColorRecord_Id" }, "ProductColorRecord", new[] { "Id" });

            return 74;
        }

        public int UpdateFrom74()
        {
            SchemaBuilder.AlterTable(typeof(LinkOrderCampaignProductRecord).Name,
                table => table.AddColumn<int>("ProductColorRecord_Id", c => c.Nullable()));

            SchemaBuilder.CreateForeignKey("LinkOrderCampaignProduct_ProductColor", "LinkOrderCampaignProductRecord",
                new[] { "ProductColorRecord_Id" }, "ProductColorRecord", new[] { "Id" });

            return 75;
        }

        public int UpdateFrom75()
        {
            SchemaBuilder.CreateTable(typeof(DeliverySettingRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("State")
                    .Column<double>("DeliveryCost", column => column.NotNull().WithDefault(0.0))
                );

            return 76;
        }

        public int UpdateFrom76()
        {
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name, table => table.DropColumn("PaymentMethod"));

            return 77;
        }

        public int UpdateFrom77()
        {
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<bool>("CashDeliv", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<bool>("PayPal", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<bool>("Mol", c => c.NotNull().WithDefault(false)));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<bool>("CreditCard", c => c.NotNull().WithDefault(false)));

            return 78;
        }

        public int UpdateFrom78()
        {
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("MerchantIdMol", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("VerifyKey", c => c.Nullable()));

            return 79;
        }

        public int UpdateFrom79()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<bool>("IsArchived", c => c.NotNull().WithDefault(false)));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<int>("BaseCampaignId", c => c.Nullable()));

            SchemaBuilder.CreateForeignKey("CampaignRecord_BaseCampaignId", "CampaignRecord",
                new[] { "BaseCampaignId" }, "CampaignRecord", new[] { "Id" });

            return 80;
        }

        public int UpdateFrom80()
        {
            SchemaBuilder.AlterTable(typeof(CampaignProductRecord).Name,
                table => table.AddColumn<DateTime>("WhenDeleted", c => c.Nullable()));

            return 81;
        }

        public int UpdateFrom81()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<int>("CntFrontColor"));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<int>("CntBackColor"));


            return 82;
        }

        public int UpdateFrom82()
        {
            SchemaBuilder.AlterTable(typeof(DeliverySettingRecord).Name,
                table => table.AddColumn<bool>("Enabled", c => c.NotNull().WithDefault(false)));
            return 83;
        }

        public int UpdateFrom83()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table =>
                    table.AddColumn<string>("CampaignCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            return 84;
        }

        public int UpdateFrom84()
        {
            SchemaBuilder.AlterTable(typeof(CurrencyRecord).Name,
                table =>
                    table.AddColumn<string>("CurrencyCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            return 85;
        }

        public int UpdateFrom85()
        {
            SchemaBuilder.AlterTable(typeof(TShirtCostRecord).Name,
                table => table.AddColumn<string>("CostCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(CampaignCategoriesRecord).Name,
                table =>
                    table.AddColumn<string>("CategoriesCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            return 86;
        }

        public int UpdateFrom86()
        {
            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table.AddColumn<string>("CashOnDeliveryAvailabilityMessage"));

            return 87;
        }

        public int UpdateFrom87()
        {
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table =>
                    table.AddColumn<string>("TeeyootUserCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            return 88;
        }

        public int UpdateFrom88()
        {
            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table.DropColumn("CashOnDeliveryAvailabilityMessage"));

            return 89;
        }

        public int UpdateFrom89()
        {
            SchemaBuilder.AlterTable(typeof(ArtRecord).Name,
                table => table.AddColumn<string>("ArtCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(ProductImageRecord).Name,
                table => table.AddColumn<string>("ProdImgCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(ProductHeadlineRecord).Name,
                table =>
                    table.AddColumn<string>("ProdHeadCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(ProductSizeRecord).Name,
                table =>
                    table.AddColumn<string>("ProdSizeCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(ProductGroupRecord).Name,
                table =>
                    table.AddColumn<string>("ProdGroupCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(FontRecord).Name,
                table => table.AddColumn<string>("FontCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            return 90;
        }

        public int UpdateFrom90()
        {
            SchemaBuilder.AlterTable(typeof(ProductColorRecord).Name,
                table =>
                    table.AddColumn<string>("ProdColorCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(SwatchRecord).Name,
                table => table.AddColumn<string>("SwatchCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            return 91;
        }

        public int UpdateFrom91()
        {
            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table.AddColumn<string>("CommonCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            return 92;
        }

        public int UpdateFrom92()
        {
            SchemaBuilder.AlterTable(typeof(DeliverySettingRecord).Name,
                table =>
                    table.AddColumn<string>("DeliveryCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            return 93;
        }

        public int UpdateFrom93()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<double>("Delivery"));

            return 94;
        }

        public int UpdateFrom94()
        {
            SchemaBuilder.CreateTable(typeof(MailTemplateSubjectRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("TemplateName")
                    .Column<string>("Culture")
                    .Column<string>("Subject")
                );

            return 95;
        }

        public int UpdateFrom95()
        {
            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table.AddColumn<string>("CashOnDeliveryAvailabilityMessage"));
            //
            // Tab names for payment methods
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("CashDelivTabName"));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("PayPalTabName"));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("MolTabName"));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("CreditCardTabName"));
            // Notes for payment methods
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("CashDelivNote"));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("PayPalNote"));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("MolNote"));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<string>("CreditCardNote"));
            //
            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table.AddColumn<string>("CheckoutPageRightSideContent", c => c.Unlimited()));

            return 96;
        }

        public int UpdateFrom96()
        {
            SchemaBuilder.AlterTable(typeof(CurrencyRecord).Name,
                table => table.AddColumn<string>("FlagFileName", c => c.WithLength(1024)));

            SchemaBuilder.CreateTable(typeof(CountryRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Code", c => c.WithLength(10))
                    .Column<string>("Name", c => c.WithLength(150))
                    .Column<int>("DefaultCulture_id")
                );

            SchemaBuilder.CreateTable(typeof(LinkCountryCurrencyRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("CurrencyRecord_Id")
                    .Column<int>("CountryRecord_Id")
                );

            SchemaBuilder.CreateTable(typeof(LinkCountryCultureRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("CultureRecord_Id")
                    .Column<int>("CountryRecord_Id")
                );

            SchemaBuilder.CreateForeignKey("LinkCountryCurrency_Currency", "LinkCountryCurrencyRecord",
                new[] { "CurrencyRecord_Id" }, "CountryRecord", new[] { "Id" });


            SchemaBuilder.CreateForeignKey("LinkCountryCurrency_Country", "LinkCountryCurrencyRecord",
                new[] { "CountryRecord_Id" }, "CountryRecord", new[] { "Id" });

            SchemaBuilder.CreateForeignKey("LinkCountryCulture_Culture", "LinkCountryCultureRecord",
                new[] { "CultureRecord_Id" }, "CountryRecord", new[] { "Id" });


            SchemaBuilder.CreateForeignKey("LinkCountryCulture_Country", "LinkCountryCultureRecord",
                new[] { "CountryRecord_Id" }, "CountryRecord", new[] { "Id" });

            return 97;

            //todo: (auth:Juiceek) apply this after all the logic in the code will be unleashed from cultures to the new business logic based on countries
            //SchemaBuilder.AlterTable(typeof(CurrencyRecord).Name, table => table.DropColumn("CurrencyCulture"));
        }

        public int UpdateFrom97()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<int>("CountryRecord_Id", c => c.WithDefault(1)));

            SchemaBuilder.CreateForeignKey("Campaign_Currency", "CampaignRecord",
                new[] { "CountryRecord_Id" }, "CountryRecord", new[] { "Id" });

            //TODO: (auth:keinlekan) Удалить колонку после того, как заработает полностью новая логика по привязке к странам
            //SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.DropColumn("CampaignCulture"));

            return 98;
        }

        public int UpdateFrom98()
        {
            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table.AddColumn<int>("CountryRecord_Id", c => c.WithDefault(1)));

            SchemaBuilder.CreateForeignKey("CommonSettings_Currency", "CommonSettingsRecord",
                new[] { "CountryRecord_Id" }, "CountryRecord", new[] { "Id" });

            //TODO: (auth:keinlekan) Удалить колонку после того, как заработает полностью новая логика по привязке к странам
            //SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name, table => table.DropColumn("CommonCulture"));

            return 99;
        }

        public int UpdateFrom99()
        {
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<int>("CountryRecord_Id", c => c.WithDefault(1)));

            SchemaBuilder.CreateForeignKey("PaymentSettings_Currency", "PaymentSettingsRecord",
                new[] { "CountryRecord_Id" }, "CountryRecord", new[] { "Id" });

            //TODO: (auth:keinlekan) Удалить колонку после того, как заработает полностью новая логика по привязке к странам
            //SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name, table => table.DropColumn("CountryRecord_Id"));

            return 100;
        }

        public int UpdateFrom100()
        {
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<int>("CurrencyRecord_Id"));

            SchemaBuilder.CreateForeignKey("TeeyootUserPartRecord_CurrencyRecord", "TeeyootUserPartRecord",
                new[] { "CurrencyRecord_Id" }, "CurrencyRecord", new[] { "Id" });

            return 101;
        }

        public int UpdateFrom101()
        {
            SchemaBuilder.AlterTable(typeof(PromotionRecord).Name,
                table => table.AddColumn<DateTime>("Created", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PromotionRecord).Name,
                table => table.AddColumn<int>("CampaignId", c => c.Nullable()));

            return 102;
        }

        public int UpdateFrom102()
        {
            SchemaBuilder.DropForeignKey("LinkCountryCurrencyRecord", "LinkCountryCurrency_Currency");
            SchemaBuilder.DropForeignKey("LinkCountryCurrencyRecord", "LinkCountryCurrency_Country");

            SchemaBuilder.DropForeignKey("LinkCountryCultureRecord", "LinkCountryCulture_Culture");
            SchemaBuilder.DropForeignKey("LinkCountryCultureRecord", "LinkCountryCulture_Country");


            SchemaBuilder.CreateForeignKey("LinkCountryCurrency_Currency", "LinkCountryCurrencyRecord",
                new[] { "CurrencyRecord_Id" }, "CurrencyRecord", new[] { "Id" });

            SchemaBuilder.CreateForeignKey("LinkCountryCurrency_Country", "LinkCountryCurrencyRecord",
                new[] { "CountryRecord_Id" }, "CountryRecord", new[] { "Id" });


            SchemaBuilder.CreateForeignKey("LinkCountryCulture_Country", "LinkCountryCultureRecord",
                new[] { "CountryRecord_Id" }, "CountryRecord", new[] { "Id" });

            return 103;
        }

        public int UpdateFrom103()
        {
            SchemaBuilder.AlterTable(typeof(CampaignCategoriesRecord).Name,
                table => table.AddColumn<int>("CountryRecord_Id", c => c.WithDefault(1)));

            SchemaBuilder.CreateForeignKey("CampaignCategories_Currency", "CampaignCategoriesRecord",
                new[] { "CountryRecord_Id" }, "CountryRecord", new[] { "Id" });

            //TODO: (auth:keinlekan) Удалить колонку после того, как заработает полностью новая логика по привязке к странам
            //SchemaBuilder.AlterTable(typeof(CampaignCategoriesRecord).Name, table => table.DropColumn("CountryRecord_Id"));

            return 104;
        }

        public int UpdateFrom104()
        {
            SchemaBuilder.AlterTable(typeof(CurrencyRecord).Name,
                table => table.AddColumn<double>("PriceBuyers", c => c.WithDefault(1)));

            SchemaBuilder.AlterTable(typeof(CurrencyRecord).Name,
                table => table.AddColumn<double>("PriceSellers", c => c.WithDefault(1)));

            SchemaBuilder.AlterTable(typeof(CurrencyRecord).Name,
                table => table.AddColumn<bool>("IsConvert", c => c.WithDefault(false)));

            return 105;
        }

        public int UpdateFrom105()
        {
            ContentDefinitionManager.AlterPartDefinition(
                "AllCountryWidgetPart",
                builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition(
                "AllCountryWidget",
                cfg => cfg
                    .WithPart("AllCountryWidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 106;
        }

        public int UpdateFrom106()
        {
            // Default is used only to set existing records Country field to 1 before applying notnull constaraint.
            //SchemaBuilder.AlterTable(typeof (DeliverySettingRecord).Name,
            //    table => table.AddColumn<int>("Country_id", c => c.WithDefault(1).NotNull()));
            SchemaBuilder.CreateForeignKey("DeliverySettings_Country",
                typeof(DeliverySettingRecord).Name, new[] { "Country_id" },
                typeof(CountryRecord).Name, new[] { "Id" });

            SchemaBuilder.AlterTable(typeof(DeliverySettingRecord).Name,
                table => table.AddColumn<double>("PostageCost"));

            SchemaBuilder.AlterTable(typeof(DeliverySettingRecord).Name,
                table => table.AddColumn<double>("CodCost"));

            // Dropping the default constarint for Country field
            SchemaBuilder.ExecuteSql(
                @"declare @table_name nvarchar(256)
                    declare @col_name nvarchar(256)
                    declare @Command  nvarchar(1000)

                    set @table_name = N'Teeyoot_Module_DeliverySettingRecord'
                    set @col_name = N'Country_id'

                    select @Command = 'ALTER TABLE ' + @table_name + ' drop constraint ' + d.name
                     from sys.tables t   
                      join    sys.default_constraints d       
                       on d.parent_object_id = t.object_id  
                      join    sys.columns c      
                       on c.object_id = t.object_id      
                        and c.column_id = d.parent_column_id
                     where t.name = @table_name
                      and c.name = @col_name

                    execute (@Command)"
                );

            return 107;
        }

        public int UpdateFrom107()
        {
            SchemaBuilder.CreateTable(typeof(DeliveryInternationalSettingRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("CountryFrom_Id", c => c.NotNull())
                    .Column<int>("CountryTo_Id", c => c.NotNull())
                    .Column<double>("DeliveryPrice")
                    .Column<bool>("IsActive", c => c.NotNull())
                );

            SchemaBuilder.CreateForeignKey("FK_DeliveryInternationalSetting_CountryFrom",
                typeof(DeliveryInternationalSettingRecord).Name, new[] { "CountryFrom_Id" },
                typeof(CountryRecord).Name, new[] { "Id" });

            SchemaBuilder.CreateForeignKey("FK_DeliveryInternationalSetting_CountryTo",
                typeof(DeliveryInternationalSettingRecord).Name, new[] { "CountryTo_Id" },
                typeof(CountryRecord).Name, new[] { "Id" });

            return 108;
        }

        public int UpdateFrom108()
        {
            SchemaBuilder.AlterTable(typeof(PayoutRecord).Name,
                table => table.AddColumn<bool>("IsProfitPaid", c => c.WithDefault(false)));

            return 109;
        }

        public int UpdateFrom109()
        {
            SchemaBuilder.AlterTable(typeof(PayoutRecord).Name,
                table => table.AddColumn<bool>("IsCampiaign", c => c.WithDefault(false)));

            return 110;
        }

        public int UpdateFrom110()
        {
            SchemaBuilder.AlterTable(typeof(PayoutRecord).Name,
                table => table.AddColumn<bool>("IsOrder", c => c.WithDefault(false)));

            return 111;
        }

        //(auth:juiceek)
        public int UpdateFrom111()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<int>("CurrencyRecord_Id"));

            SchemaBuilder.CreateForeignKey("FK_Campaign_Currency",
                typeof(CampaignRecord).Name, new[] { "CurrencyRecord_Id" },
                typeof(CurrencyRecord).Name, new[] { "Id" });

            SchemaBuilder.ExecuteSql(@"
                    update c
                        set c.CurrencyRecord_Id = p.CurrencyRecord_Id
                    from 
                    [dbo].[Teeyoot_Module_CampaignRecord] c
                        left outer join 
                    [dbo].[Teeyoot_Module_CampaignProductRecord] p
	                    on p.CampaignRecord_Id = c.Id
                ");

            return 112;
        }

        public int UpdateFrom112()
        {
            SchemaBuilder.AlterTable(typeof(PromotionRecord).Name,
                table => table.AddColumn<int>("CurrencyRecord_Id"));

            SchemaBuilder.CreateForeignKey("Promotion_Currency",
                "PromotionRecord", new[] { "CurrencyRecord_Id" },
                "CurrencyRecord", new[] { "Id" });

            return 113;
        }

        public int UpdateFrom113()
        {
            //SchemaBuilder.AlterTable(typeof(Orchard.Users.Models.UserPartRecord).Name,

            SchemaBuilder.ExecuteSql(@"
                    ALTER TABLE [Orchard_Users_UserPartRecord]
                    ADD CultureRecord_Id int NULL;
                ");

            SchemaBuilder.CreateForeignKey("FK_UserPartRecord_CultureRecord",
                "Orchard.Users",
                typeof(Orchard.Users.Models.UserPartRecord).Name, new[] { "CultureRecord_Id" },
                "Orchard.Framework",
                typeof(Orchard.Localization.Records.CultureRecord).Name, new[] { "Id" });

            return 114;
        }

        public int UpdateFrom114()
        {
            SchemaBuilder.CreateForeignKey("LinkCountryCulture_Culture",
                "Teeyoot.Module",
                "LinkCountryCultureRecord", new[] { "CultureRecord_Id" },
                "Orchard.Framework",
                typeof(Orchard.Localization.Records.CultureRecord).Name, new[] { "Id" });

            return 115;
        }

        public int UpdateFrom115()
        {
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<int>("CountryRecord_Id"));

            SchemaBuilder.CreateForeignKey("TeeyootUserPartRecord_CountryRecord",
                "TeeyootUserPartRecord", new[] { "CountryRecord_Id" },
                "CountryRecord", new[] { "Id" });

            return 116;
        }

        public int UpdateFrom116()
        {
            SchemaBuilder.CreateTable(typeof(CurrencyExchangeRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("CurrencyFrom_Id", c => c.NotNull())
                    .Column<int>("CurrencyTo_Id", c => c.NotNull())
                    .Column<double>("RateForBuyer")
                    .Column<double>("RateForSeller")
                );

            SchemaBuilder.CreateForeignKey("FK_CurrencyExchange_CurrencyFrom",
                typeof(CurrencyExchangeRecord).Name, new[] { "CurrencyFrom_Id" },
                typeof(CurrencyRecord).Name, new[] { "Id" });

            SchemaBuilder.CreateForeignKey("FK_CurrencyExchange_CurrencyTo",
                typeof(CurrencyExchangeRecord).Name, new[] { "CurrencyTo_Id" },
                typeof(CurrencyRecord).Name, new[] { "Id" });

            return 117;
        }

        public int UpdateFrom117()
        {




            //SchemaBuilder.AlterTable(typeof (OrderRecord).Name,
            //    table => table.AddColumn<int>("SellerCountry_Id"));



            return 118;
        }

        public int UpdateFrom118()
        {
            SchemaBuilder.CreateForeignKey("CountryRecord_LinkCountryCultureRecord",
                "CountryRecord", new[] { "DefaultCulture_Id" },
                "LinkCountryCultureRecord", new[] { "Id" });


            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
              table => table.AddColumn<string>("DefaultFBPixelId"));
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("DefaultTwitterPixelId"));
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("DefaultPinterestPixelId"));
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("DefaultGooglePixelId"));
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("DefaultGoogleLabelPixelId"));
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("DefaultGoogleAnalyticsTrackingSnippet"));
            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
                table => table.AddColumn<string>("DefaultFacebookCustomAudiencePixel"));

            return 119;

        }

        //public int UpdateFrom119()
        //{


        //    return 120;
        //}

        //public int UpdateFrom120()
        //{


        //    return 121;
        //}

        //public int UpdateFrom121()
        //{


        //    return 122;
        //}

        //public int UpdateFrom124()
        //{
        //    SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
        //        table => table.AddColumn<string>("FBPixelId"));
        //    return 125;
        //}

        public int UpdateFrom119()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                table => table.AddColumn<string>("PinterestPixelId"));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
               table => table.AddColumn<string>("GooglePixelId"));
            return 120;
        }


        //public int UpdateFrom127()
        //{

        //    return 128;
        //}

        public int UpdateFrom120()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                table => table.AddColumn<int>("BuyerCultureRecord_Id"));

            SchemaBuilder.CreateForeignKey("FK_OrderRecord_CultureRecord",
                "Teeyoot.Module",
                typeof(OrderRecord).Name, new[] { "BuyerCultureRecord_Id" },
                "Orchard.Framework",
                typeof(CultureRecord).Name, new[] { "Id" });

            return 121;
        }

        //public int UpdateFrom129()
        //{


        //    return 130;
        //}

        public int UpdateFrom121()
        {
            SchemaBuilder.AlterTable(typeof(CheckoutCampaignRequest).Name,
                table => table.AddColumn<int>("BuyerCultureRecord_Id"));

            SchemaBuilder.CreateForeignKey("FK_CheckoutCampaignRequest_CultureRecord",
                "Teeyoot.Module",
                typeof(CheckoutCampaignRequest).Name, new[] { "BuyerCultureRecord_Id" },
                "Orchard.Framework",
                typeof(CultureRecord).Name, new[] { "Id" });


            SchemaBuilder.AlterTable(typeof(BringBackCampaignRecord).Name,
                table => table.AddColumn<int>("BuyerCultureRecord_Id"));

            SchemaBuilder.CreateForeignKey("FK_BringBackCampaignRecord_CultureRecord",
                "Teeyoot.Module",
                typeof(BringBackCampaignRecord).Name, new[] { "BuyerCultureRecord_Id" },
                "Orchard.Framework",
                typeof(CultureRecord).Name, new[] { "Id" });

            return 122;
        }

        public int UpdateFrom122()
        {

            return 123;
        }


        public int UpdateFrom123()
        {

            //SchemaBuilder.CreateForeignKey("OrderRecord_SellerCountry",
            //    "OrderRecord", new[] { "SellerCountry_Id" },
            //    "CountryRecord", new[] { "Id" });

            return 124;

            //var malaysia = _countryRepository.Table
            //   .First(c => c.Code == "MY");
            //var currency = malaysia.CountryCurrencies.First().CurrencyRecord;

            //var sellers = _teeyootUserPartRepository.Table.ToList();

            //foreach (var seller in sellers)
            //{
            //    seller.CurrencyRecord = currency;
            //    _teeyootUserPartRepository.Update(seller);
            //}
            //var campaigns = _campaignRepository.Table.ToList();
            //foreach (var campaign in campaigns)
            //{
            //    campaign.CountryRecord = malaysia;
            //    campaign.CurrencyRecord = currency;

            //    _campaignRepository.Update(campaign);
            //}


            //SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
            //    table => table.AddColumn<int>("SellerCurrency_Id"));

            //SchemaBuilder.CreateForeignKey("OrderRecord_CurrencyRecord",
            //    "OrderRecord", new[] { "SellerCurrency_Id" },
            //    "CurrencyRecord", new[] { "Id" });

            //SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
            //    table => table.AddColumn<double>("ExchangeRate", c => c.Nullable()));
            //var enMYculture = _cultureRepository.Table.First(c => c.Culture.ToUpper() == "EN-MY");

            //foreach (var order in _orderRepository.Table)
            //{
            //    order.BuyerCultureRecord = enMYculture;
            //    _orderRepository.Update(order);
            //}

            //foreach (var user in _userPartRepository.Table)
            //{
            //    user.CultureRecord = enMYculture;
            //    _userPartRepository.Update(user);
            //}



            //foreach (var seller in sellers)
            //{
            //    seller.CountryRecord = malaysia;
            //    _teeyootUserPartRepository.Update(seller);
            //}

            //SchemaBuilder.ExecuteSql("UPDATE Teeyoot_Module_CheckoutCampaignRequest set BuyerCultureRecord_Id=" +
            //                         enMYculture.Id);
            //SchemaBuilder.ExecuteSql("UPDATE Teeyoot_Module_BringBackCampaignRecord set BuyerCultureRecord_Id=" +
            //                         enMYculture.Id);

            //var countries = _countryRepository.Table.ToList();

            //foreach (var country in countries)
            //{
            //    var defaultCountryCulture = country.CountryCultures
            //        .FirstOrDefault(c => c.CultureRecord == enMYculture);

            //    if (defaultCountryCulture == null)
            //    {
            //        defaultCountryCulture = new LinkCountryCultureRecord
            //        {
            //            CountryRecord = country,
            //            CultureRecord = enMYculture
            //        };

            //        country.CountryCultures.Add(defaultCountryCulture);
            //    }

            //    country.DefaultCulture = defaultCountryCulture;

            //    _countryRepository.Update(country);
            //}


            //return 119;
        }


        public int UpdateFrom124()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
             table => table.AddColumn<int>("SellerCurrency_Id"));

            SchemaBuilder.CreateForeignKey("OrderRecord_CurrencyRecord",
                "OrderRecord", new[] { "SellerCurrency_Id" },
                "CurrencyRecord", new[] { "Id" });

            return 125;
        }
        public int UpdateFrom125()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
           table => table.AddColumn<string>("FBPixelId"));
            return 126;
        }
        public int UpdateFrom126()
        {
            //SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
            //  table => table.AddColumn<double>("ExchangeRate", c => c.Nullable()));
            return 127;
        }
        public int UpdateFrom127()
        {
            var malaysia = _countryRepository.Table
               .First(c => c.Code == "MY");
            var currency = malaysia.CountryCurrencies.First().CurrencyRecord;

            var sellers = _teeyootUserPartRepository.Table.ToList();

            foreach (var seller in sellers)
            {
                seller.CurrencyRecord = currency;
                _teeyootUserPartRepository.Update(seller);
            }
            var campaigns = _campaignRepository.Table.ToList();
            foreach (var campaign in campaigns)
            {
                campaign.CountryRecord = malaysia;
                campaign.CurrencyRecord = currency;

                _campaignRepository.Update(campaign);
            }

            var enMYculture = _cultureRepository.Table.First(c => c.Culture.ToUpper() == "EN-MY");

            foreach (var order in _orderRepository.Table)
            {
                order.BuyerCultureRecord = enMYculture;
                _orderRepository.Update(order);
            }

            foreach (var user in _userPartRepository.Table)
            {
                user.CultureRecord = enMYculture;
                _userPartRepository.Update(user);
            }



            foreach (var seller in sellers)
            {
                seller.CountryRecord = malaysia;
                _teeyootUserPartRepository.Update(seller);
            }

            SchemaBuilder.ExecuteSql("UPDATE Teeyoot_Module_CheckoutCampaignRequest set BuyerCultureRecord_Id=" +
                                     enMYculture.Id);
            SchemaBuilder.ExecuteSql("UPDATE Teeyoot_Module_BringBackCampaignRecord set BuyerCultureRecord_Id=" +
                                     enMYculture.Id);

            var countries = _countryRepository.Table.ToList();

            foreach (var country in countries)
            {
                var defaultCountryCulture = country.CountryCultures
                    .FirstOrDefault(c => c.CultureRecord == enMYculture);

                if (defaultCountryCulture == null)
                {
                    defaultCountryCulture = new LinkCountryCultureRecord
                    {
                        CountryRecord = country,
                        CultureRecord = enMYculture
                    };

                    country.CountryCultures.Add(defaultCountryCulture);
                }

                country.DefaultCulture = defaultCountryCulture;

                _countryRepository.Update(country);
            }

            return 128;
        }

        public int UpdateFrom128()
        {
            SchemaBuilder.AlterTable(typeof(ProductColorRecord).Name,
              table =>
                  table.AddColumn<string>("ProdColorCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(SwatchRecord).Name,
                table => table.AddColumn<string>("SwatchCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            return 129;
        }

        public int UpdateFrom129()
        {
            SchemaBuilder.AlterTable(typeof(ProductHeadlineRecord).Name,
                table =>
                    table.AddColumn<string>("ProdHeadCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(ProductSizeRecord).Name,
                table =>
                    table.AddColumn<string>("ProdSizeCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(ProductGroupRecord).Name,
                table =>
                    table.AddColumn<string>("ProdGroupCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            SchemaBuilder.AlterTable(typeof(FontRecord).Name,
                table => table.AddColumn<string>("FontCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));


            SchemaBuilder.AlterTable(typeof(CommonSettingsRecord).Name,
                table => table.AddColumn<string>("CommonCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            SchemaBuilder.AlterTable(typeof(DeliverySettingRecord).Name,
                table =>
                    table.AddColumn<string>("DeliveryCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));

            return 130;
        }

        public int UpdateFrom130()
        {
            SchemaBuilder.AlterTable(typeof(ProductImageRecord).Name,
                table => table.AddColumn<string>("ProdImgCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            return 131;
        }

        public int UpdateFrom131()
        {
            SchemaBuilder.AlterTable(typeof(ArtRecord).Name,
            table => table.AddColumn<string>("ArtCulture", c => c.NotNull().WithDefault("en-MY").WithLength(10)));
            return 132;
        }

        public int UpdateFrom132()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
            table => table.AddColumn<int>("Campaign_Id", aa => aa.WithDefault(37)));

            SchemaBuilder.CreateForeignKey("OrderRecord_CampaignRecord",
                "OrderRecord", new[] { "Campaign_Id" },
                "CampaignRecord", new[] { "Id" });
            return 133;
        }
        public int UpdateFrom133()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
            table => table.AddColumn<int>("Seller_Id", aa => aa.WithDefault(450)));

            //SchemaBuilder.DropForeignKey("OrderRecord", "OrderRecord_UserSeller");
            SchemaBuilder.CreateForeignKey("OrderRecord_UserSeller",
                "Teeyoot_Module", "OrderRecord", new[] { "Seller_Id" },
                "Orchard_Users", "UserPartRecord", new[] { "Id" });
            return 134;
        }
        public int UpdateFrom134()
        {
            foreach (OrderRecord order in _orderRepository.Table)
            {
                if (order.Email == null) { continue; }
                if (order.Products == null || order.Products.Count() <= 0 || order.Products.First().CampaignProductRecord == null) { continue; }
                order.Campaign = _campaignRepository.Get(order.Products.First().CampaignProductRecord.CampaignRecord_Id);
                _orderRepository.Update(order);

            }
            return 135;
        }



        public int UpdateFrom135()
        {
            foreach (OrderRecord order in _orderRepository.Table)
            {
                if (order.Campaign == null) continue;
                order.Seller = _userPartRepository.Get(order.Campaign.TeeyootUserId.Value);
                _orderRepository.Update(order);
            }
            return 136;
        }

        public int UpdateFrom136()
        {
            SchemaBuilder.AlterTable(typeof(MailTemplateSubjectRecord).Name,
                table => table.AddColumn<string>("Culture", c => c.WithDefault("en-MY")));
            return 137;
        }

        public int UpdateFrom137()
        {
            foreach (OrderRecord order in _orderRepository.Table)
            {
                if (order.Email == null) { continue; }
                if (order.Products == null || order.Products.Count() <= 0 || order.Products.First().CampaignProductRecord == null) { continue; }
                order.Campaign = _campaignRepository.Get(order.Products.First().CampaignProductRecord.CampaignRecord_Id);
                _orderRepository.Update(order);

            }
            return 138;
        }

        public int UpdateFrom138()
        {
            ContentDefinitionManager.AlterPartDefinition(
                "SelectCurrencyWidgetPart",
                builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition(
                "SelectCurrencyWidget",
                cfg => cfg
                    .WithPart("SelectCurrencyWidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 139;
        }

        public int UpdateFrom139()
        {
            ContentDefinitionManager.DeletePartDefinition("SelectCurrencyWidgetPart");

            ContentDefinitionManager.AlterPartDefinition(
               "SelectCurrencyWidgetPart",
               builder => builder.Attachable());

            ContentDefinitionManager.AlterTypeDefinition(
                "SelectCurrencyWidget",
                cfg => cfg
                    .WithPart("SelectCurrencyWidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );
            return 140;
        }

        public int UpdateFrom140()
        {
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                table => table.AddColumn<bool>("Ipay88", c => c.Nullable()));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
               table => table.AddColumn<string>("Ipay88MerchantCode", c => c.Nullable()));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
               table => table.AddColumn<int>("Ipay88PaymentId", c => c.Nullable()));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
               table => table.AddColumn<string>("Ipay88TabName", c => c.Nullable()));
            return 141;
        }

        public int UpdateFrom141()
        {
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                          table => table.AddColumn<string>("Ipay88MerchantKey", c => c.Nullable()));
            return 142;
        }

        public int UpdateFrom142()
        {
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                         table => table.AddColumn<bool>("Paypal_", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                        table => table.AddColumn<string>("PaypalTabName_", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                        table => table.AddColumn<string>("PaypalClientID_", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
                        table => table.AddColumn<string>("PaypalSecret_", c => c.Nullable()));

            return 143;
        }

        public int UpdateFrom143()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                         table => table.AddColumn<string>("PaymentMethod", c => c.Nullable().WithDefault("COD")));

            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                       table => table.AddColumn<int>("TotalSold", c => c.Nullable().WithDefault(0)));

            SchemaBuilder.CreateTable(typeof(Outbox).Name,
               table => table
                   .Column<int>("Id", column => column.PrimaryKey().Identity())
                   .Column<string>("Data", c => c.Unlimited().Nullable())
                   .Column<DateTime>("Added", c => c.Nullable())
               );

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<bool>("CampaignEndAndDelivered", c => c.Nullable().WithDefault(false)));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<int>("NumOfApprovedButNotDeliverd", c => c.Nullable().WithDefault(0)));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<double>("UnclaimableProfit", c => c.Nullable().WithDefault(0)));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<double>("ClaimableProfit", c => c.Nullable().WithDefault(0)));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<bool>("CampaignEndButNotDeliverd", c => c.Nullable().WithDefault(0)));


            //SchemaBuilder.AlterTable()
            return 144;
        }

        public int UpdateFrom144()
        {
            foreach (var order in _orderRepository.Table.Where(aa => aa.Email != null || aa.IsActive))
            {
                order.PaymentMethod = order.Paid == null ? "COD" : "MOL";
                _orderRepository.Update(order);
            }
            return 145;
        }


        public int UpdateFrom145()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                         table => table.AddColumn<int>("Seller_Id", c => c.Nullable().WithDefault(0)));

            SchemaBuilder.CreateForeignKey("CampaignRecord_UserSeller",
               "Teeyoot_Module", "CampaignRecord", new[] { "Seller_Id" },
               "Orchard_Users", "UserPartRecord", new[] { "Id" });
            return 146;
        }
        //public int UpdateFrom146()
        //{
        //    foreach (var campagin_id in _campaignRepository.Table.Select(aa => new { id = aa.Id }))
        //    {
        //        var campagin = _campaignRepository.Get(campagin_id.id);
        //        campagin.Seller = (campagin.TeeyootUserId.HasValue) ? _userPartRepository.Get(campagin.TeeyootUserId.Value) : _userPartRepository.Get(0);
        //        _campaignRepository.Update(campagin);
        //    }
        //    return 147;
        //}
        public int UpdateFrom146()
        {
           
            return 147;
        }


        public int UpdateFrom147()
        {
            SchemaBuilder.CreateTable(typeof(CampaignDesign).Name,
               table => table
                   .Column<int>("Id", column => column.PrimaryKey().Identity())
                   .Column<string>("Data", c => c.Unlimited().Nullable())
               );
           
            return 148;
        }
        public int UpdateFrom148()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                        table => table.AddColumn<int>("CampaignDesign_Id", c => c.Nullable().WithDefault(0)));

            SchemaBuilder.CreateForeignKey("CampaignRecord_CampaignDesign",
              "Teeyoot_Module", "CampaignRecord", new[] { "CampaignDesign_Id" },
              "Teeyoot_Module", "CampaignDesign", new[] { "Id" });
            return 149;
        }

        public int UpdateFrom149()
        {
            foreach (var campagin_id in _campaignRepository.Table.Select(aa => new { id = aa.Id }))
            {
                var campagin = _campaignRepository.Fetch(aa => aa.Id == campagin_id.id).First();
                campagin.Seller = (campagin.TeeyootUserId.HasValue) ? _userPartRepository.Get(campagin.TeeyootUserId.Value) : _userPartRepository.Get(0);
                _campaignRepository.Update(campagin);
            }
            return 150;
        }


        public int UpdateFrom150()
        {
            //SchemaBuilder.AlterTable(typeof(OrderRecord).Name, table => table.DropColumn("TotalSold"));
            return 151;
        }

        public int UpdateFrom151()
        {
           
            return 152;
        }



        public int UpdateFrom152()
        {
            foreach (var order_id in _orderRepository.Table.Select(aa => new { id = aa.Id }))
            {
                var order  = _orderRepository.Fetch(aa => aa.Id == order_id.id).First();

                if (order.Products == null || order.Products.Count == 0 || string.IsNullOrWhiteSpace(order.Email) || order.OrderStatusRecord == null) { order.IsActive = false; _orderRepository.Update(order); continue; }
                //if (order.OrderStatusRecord.Name == "Cancelled" || order.OrderStatusRecord.Name == "Refunded" || order.OrderStatusRecord.Name == "Pending") continue;
                var soldCount = 0;
                foreach (var product in order.Products)
                    soldCount += product.Count;
                order.TotalSold = soldCount;
                _orderRepository.Update(order);
            }
            return 153;
        }

        public int UpdateFrom153()
        {
            //ProductCountSold
            foreach (var campaign in _campaignRepository.Fetch(aa => aa.WhenDeleted == null))
            {
                var orders = _orderRepository.Fetch(aa => aa.Campaign.Id == campaign.Id && (aa.OrderStatusRecord.Name == "Approved" ||
                        aa.OrderStatusRecord.Name == "Printing" ||
                        aa.OrderStatusRecord.Name == "Shipped" ||
                        aa.OrderStatusRecord.Name == "Delivered"));

                campaign.ProductCountSold = orders.Select(aa => aa.TotalSold).Sum();
                _campaignRepository.Update(campaign);

            }
            return 154;
        }

        public int UpdateFrom154()
        {
            
            return 155;
        }

        public int UpdateFrom155()
        {
           
            return 156;
        }


        public int UpdateFrom156()
        {
            SchemaBuilder.AlterTable(typeof(PayoutRecord).Name,
             table => table.AddColumn<int>("CampaignId", c => c.Nullable()));
            SchemaBuilder.AlterTable(typeof(PayoutRecord).Name,
             table => table.AddColumn<int>("OrderId", c => c.Nullable()));
            
            return 157;
        }

        public int UpdateFrom157()
        {
            SchemaBuilder.AlterTable(typeof(PayoutRecord).Name,
            table => table.AddColumn<string>("Description", c => c.Nullable()));
            return 158;
        }


        public int UpdateFrom158()
        {
            SchemaBuilder.AlterTable(typeof(TShirtCostRecord).Name,
                        table => table.AddColumn<int>("CostRevision", c => c.Nullable().WithDefault(7)));



            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                                   table => table.AddColumn<int>("TShirtCostRecord_Id", c => c.Nullable().WithDefault(7)));

            SchemaBuilder.CreateForeignKey("CampaignRecord_TShirtCostRecord",
             "Teeyoot_Module", "CampaignRecord", new[] { "TShirtCostRecord_Id" },
             "Teeyoot_Module", "TShirtCostRecord", new[] { "Id" });
            return 159;
        }

        public int UpdateFrom159()
        {
            SchemaBuilder.AlterTable(typeof(TShirtCostRecord).Name, table => table.AddColumn<DateTime>("DateCreated", c => c.Nullable().WithDefault(new DateTime(2014, 1, 1, 1, 1, 1, 1))));
            return 160;
        }

        public int UpdateFrom160()
        {
            //ReceiveEmail

            SchemaBuilder.AlterTable(typeof(TeeyootUserPartRecord).Name,
             table => table.AddColumn<bool>("ReceiveEmail"));

            return 161;
        }

        public int UpdateFrom161()
        {
            SchemaBuilder.AlterTable(typeof(Outbox).Name,
             table => table.AddColumn<int>("OrderId", column => column.Nullable()));
            
            SchemaBuilder.AlterTable(typeof(Outbox).Name,
                         table => table.AddColumn<string>("EmailType", column => column.Nullable()));
            return 162;

        }

        public int UpdateFrom162()
        {
            SchemaBuilder.AlterTable(typeof(CampaignProductRecord).Name,
               table => table.AddColumn<double>("CostOfMaterial", col => col.Nullable()));
            return 163;
        }

        public int UpdateFrom163()
        {
            foreach (var prod in _campaignproducts.Table)
            {
                if (prod.ProductRecord != null)
                {
                    prod.CostOfMaterial = prod.ProductRecord.BaseCost;
                }
                _campaignproducts.Update(prod);
                _campaignproducts.Flush();
            }
            return 164;
        }
        public int UpdateFrom164()
        {
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
               table => table.AddColumn<string>("PayPalNote_", c => c.Nullable()));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name,
               table => table.AddColumn<string>("Ipay88Note", c => c.Nullable()));

            return 165;

        }

        public int UpdateFrom165()
        {
            SchemaBuilder.AlterTable(typeof(ProductSizeRecord).Name,
               table => table.AddColumn<string>("Remarks", c => c.Nullable()));
            
            return 166;
        }

        public int UpdateFrom166()
        {
            SchemaBuilder.AlterTable(typeof(ProductGroupRecord).Name,
               table =>
                   table.AddColumn<int>("xOrder", c => c.Nullable()));
            return 167;
        }

        public int UpdateFrom167()
        {
            SchemaBuilder.AlterTable(typeof(ProductRecord).Name,
               table =>
                   table.AddColumn<int>("xOrder", c => c.Nullable()));
            return 168;
        }

        public int UpdateFrom168()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
                    table =>
                        table.AddColumn<double>("OrderProfit", c => c.Nullable()));
            return 169;
        }
        
        public int UpdateFrom169()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                    table =>
                        table.AddColumn<int>("SoldLast24Hours", c => c.Nullable()));
            return 170;
        }

        public int UpdateFrom170()
        {

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                  table =>
                      table.AddColumn<int>("SoldYesterDay", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                  table =>
                      table.AddColumn<int>("TotalSold", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name,
                  table =>
                      table.AddColumn<int>("ActiveSold", c => c.Nullable()));


            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<double>("Last24HoursProfit", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<double>("ActiveProfit", c => c.Nullable()));

            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<double>("TotalProfit", c => c.Nullable()));

           


            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<double>("Profit", c => c.Nullable()));
            
            return 171;
        
        }
        public int UpdateFrom171()
        { return 172; }

        public int UpdateFrom172()
        {
            //SchemaBuilder.AlterTable(typeof(CurrencyRecord).Name,
            //        table =>
            //            table.AddColumn<string>("ShortCode", c => c.Nullable()));
            return 173;
            
        }

        public int UpdateFrom173()
        {
            SchemaBuilder.AlterTable(typeof(TShirtCostRecord).Name, table => table.AddColumn<int>("MinimumTarget", c => c.Nullable().WithDefault(2)));
            return 174;
        }

        public int UpdateFrom174()
        {
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<int>("Tareget", c => c.Nullable().WithDefault(1)));
            SchemaBuilder.AlterTable(typeof(CampaignRecord).Name, table => table.AddColumn<bool>("TargetSet", c => c.Nullable().WithDefault(false)));
            return 175;
        }

        public int UpdateFrom175()
        {
            SchemaBuilder.AlterTable(typeof(DeliverySettingRecord).Name, table => table.AddColumn<int>("DeliveryTime", c => c.Nullable().WithDefault(1)));
            return 176;
        }
        public int UpdateFrom176()
        {
            SchemaBuilder.AlterTable(typeof(DeliveryInternationalSettingRecord).Name, table => table.AddColumn<int>("DeliveryTime", c => c.Nullable().WithDefault(1)));
            return 177;
        }

        public int UpdateFrom177()
        {
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name, table => table.AddColumn<bool>("BlueSnap", c => c.Nullable().WithDefault(1)));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name, table => table.AddColumn<string>("BlueSnapTabName", c => c.Nullable().WithDefault(1)));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name, table => table.AddColumn<string>("BlueSnapDesc", c => c.Nullable().WithDefault(1)));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name, table => table.AddColumn<string>("BlueSnapKey", c => c.Nullable().WithDefault(1)));
            SchemaBuilder.AlterTable(typeof(PaymentSettingsRecord).Name, table => table.AddColumn<string>("BlueSnapPass", c => c.Nullable().WithDefault(1)));
            return 178;
        }


        public int UpdateFrom178()
        {
            //:)
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name, table => table.AddColumn<string>("BlueSnapTransationId", c => c.Nullable().WithDefault(1)));
            return 179;
        }

        public int UpdateFrom179()
        {
            SchemaBuilder.AlterTable(typeof(OrderRecord).Name, table => table.AddColumn<string>("selectedCurrency", c => c.Nullable().WithDefault(1)));
            return 180;
        }
        public int UpdateFrom180()
        {
            SchemaBuilder.AlterTable(typeof(SizeCodeRecord).Name, table => table.AddColumn<string>("xOrder", c => c.Nullable().WithDefault(0)));
            return 181;
        }
        //public int UpdateFrom180()
        //{
        //    //SchemaBuilder.CreateTable(typeof(PendingTable).Name, )
        //    MailTemplateSubjectRecord m = new MailTemplateSubjectRecord()
        //    {
        //         Culture = "en-My"
        //    };

        //}

        //public int UpdateFrom171()
        //{
        //    //SchemaBuilder.AlterTable(typeof(OrderRecord).Name,
        //    //    table =>
        //    //        table.AddColumn<double>("OrderProfit", c => c.Nullable()));
        //    return 172;
        //}


        //public int UpdateFrom171()
        //{
        //    var campaigns = _campaignRepository.Table.Where(aa => aa.IsApproved == true);
        //    int i = 0;
        //    foreach (var campaign in campaigns)
        //    {
        //        i++;

        //        var orders = _orderRepository.Table.Where(aa => aa.Campaign.Id == campaign.Id && (aa.OrderStatusRecord.Name == OrderStatus.Approved.ToString() ||
        //                                                                                          aa.OrderStatusRecord.Name == OrderStatus.Delivered.ToString() ||
        //                                                                                          aa.OrderStatusRecord.Name == OrderStatus.Printing.ToString() ||
        //                                                                                          aa.OrderStatusRecord.Name == OrderStatus.Shipped.ToString()));
        //        int totalSold = 0;
        //        double totalCampaignProfit = 0;
        //        int countOfCODthatnotDeliverd = 0;
        //        double UnclaimableProfit = 0;


        //        var yesterDayOrder = orders.Where(aa => aa.Created < DateTime.Now.AddDays(-1) && aa.Created > DateTime.Now.AddDays(-2));
        //        var todayOrders = orders.Where(aa => aa.Created > DateTime.Now.AddDays(-1));

        //        Dictionary<int, float> Productcost = new Dictionary<int, float>();


        //        foreach (var product in campaign.Products)
        //        {
        //            if (Productcost.ContainsKey(product.ProductRecord.Id)) continue;
        //            Productcost.Add(product.ProductRecord.Id, _campaignservice.CalculateBaseCost(campaign.Id, product.Id, campaign.ProductCountSold));
        //        }



        //        foreach (var order in orders)
        //        {
        //            double orderTotal = (order.TotalPriceWithPromo != 0) ? order.TotalPriceWithPromo : order.TotalPrice;
        //            double orderBasePrice = 0;

        //            bool isCod = order.PaymentMethod == "COD";

        //            if (isCod)
        //            {
        //                if (order.OrderStatusRecord.Name == OrderStatus.Delivered.ToString())
        //                {
        //                    totalSold += order.TotalSold;
        //                    orderBasePrice = 0;
        //                    foreach (var item in order.Products)
        //                    {
        //                        orderBasePrice += item.Count * (Productcost[item.CampaignProductRecord.ProductRecord.Id]);
        //                    }

        //                    totalCampaignProfit += (orderTotal - orderBasePrice);
        //                }
        //                else
        //                {
        //                    if (order.OrderStatusRecord.Name != OrderStatus.Cancelled.ToString() &&
        //                        order.OrderStatusRecord.Name != OrderStatus.Refunded.ToString() &&
        //                        order.OrderStatusRecord.Name != OrderStatus.Pending.ToString())
        //                    {
        //                        totalSold += order.TotalSold;
        //                        countOfCODthatnotDeliverd++;
        //                        orderBasePrice = 0;
        //                        foreach (var item in order.Products)
        //                        {
        //                            orderBasePrice += item.Count * (Productcost[item.CampaignProductRecord.ProductRecord.Id]);
        //                        }
        //                        UnclaimableProfit += (orderTotal - orderBasePrice);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (order.OrderStatusRecord.Name != OrderStatus.Cancelled.ToString() &&
        //                        order.OrderStatusRecord.Name != OrderStatus.Refunded.ToString() &&
        //                        order.OrderStatusRecord.Name != OrderStatus.Pending.ToString())
        //                {

        //                    totalSold += order.TotalSold;
        //                    orderBasePrice = 0;
        //                    foreach (var item in order.Products)
        //                    {
        //                        orderBasePrice += item.Count * (Productcost[item.CampaignProductRecord.ProductRecord.Id]);
        //                    }
        //                    totalCampaignProfit += (orderTotal - orderBasePrice);
        //                }
        //            }

        //            order.OrderProfit = (orderTotal - orderBasePrice);
        //            _orderRepository.Update(order);

        //        }

        //        campaign.TotalSold = totalSold;
        //        campaign.TotalProfit = (totalSold < campaign.ProductMinimumGoal)? 0 : orders.Select(aa => aa.OrderProfit).ToArray().Sum();
        //        campaign.YesterDayProfit = (totalSold < campaign.ProductMinimumGoal) ? 0 : todayOrders.Select(aa => aa.OrderProfit).ToArray().Sum();
        //        campaign.Last24HoursProfit = (totalSold < campaign.ProductMinimumGoal) ? 0 : yesterDayOrder.Select(aa => aa.OrderProfit).ToArray().Sum();
        //        campaign.SoldLast24Hours = todayOrders.Select(aa => aa.TotalSold).ToArray().Sum();
        //        campaign.SoldYesterDay = yesterDayOrder.Select(aa => aa.TotalSold).ToArray().Sum();
        //        campaign.ActiveSold = (campaign.IsActive) ? 0 : campaign.TotalSold;


        //        campaign.UnclaimableProfit = (totalSold < campaign.ProductMinimumGoal) ? 0 : UnclaimableProfit < 0 ? 0 : UnclaimableProfit;
        //        campaign.NumOfApprovedButNotDeliverd = countOfCODthatnotDeliverd;
        //        campaign.ClaimableProfit = (totalSold < campaign.ProductMinimumGoal) ? 0 : (totalCampaignProfit < 0 ? 0 : totalCampaignProfit);
        //        campaign.ProductCountSold = totalSold;
        //        campaign.CampaignProfit = (totalSold < campaign.ProductMinimumGoal) ? "0" : ((totalCampaignProfit + UnclaimableProfit) < 0 ? 0 : (UnclaimableProfit + totalCampaignProfit)).ToString();

        //        _campaignRepository.Update(campaign);
        //    }
        //    return 172;
        //}

        
        //public int UpdateFrom149()
        //{

            //SchemaBuilder.ExecuteSql("", )
            //Dictionary<int, int> dics = new Dictionary<int, int>();
            //const int batchSize = 1;
            //var i = 0;
            //var transManager = _orchardServices.TransactionManager;
            //using (ISession session1 = _sessionLocator.For(typeof(CampaignDesign)))
            //{
            //    //_campaignRepository.ex
            //    while (i * batchSize < _campaignRepository.Table.Count())
            //    {
            //        using (var tx1 = session1.BeginTransaction(IsolationLevel.ReadCommitted))
            //        {
            //            var batch = _campaignRepository.Table.Skip(i++ * batchSize).Take(batchSize).Select(aa=>aa.Id);
            //            foreach (var campaginid in batch)
            //            {
            //                var campagin = _campaignRepository.Get(campaginid);
            //                if (string.IsNullOrWhiteSpace(campagin.Design) && campagin.CampaignDesign != null) continue;
            //                var campaign_design = new CampaignDesign()
            //                {
            //                    Data = campagin.Design
            //                };
            //               _campaignDesign.Create(campaign_design);
            //                //campagin.CampaignDesign = campaign_design;
            //                dics.Add(campaign_design.Id, campagin.Id);
            //            }
            //            //tx2.Commit();
            //            tx1.Commit();
            //            i++;
            //        }
            //    }
            //}



            ////update repostitoryTable

            //i = 0;

            //using (ISession session1 = _sessionLocator.For(typeof(CampaignRecord)))
            //{

            //    foreach (var item in dics)
            //    {
            //        using (var tx1 = session1.BeginTransaction(IsolationLevel.ReadCommitted))
            //        {
            //            var _campaign = _campaignRepository.Get(item.Value);
            //            _campaign.Design = "";
            //            _campaign.CampaignDesign = _campaignDesign.Get(item.Key);
            //            _campaignRepository.Update(_campaign);
            //            tx1.Commit();
            //        }
                          
            //    }
                //while (i * batchSize < dics.Count)
                //{
                //    using (var tx1 = session1.BeginTransaction(IsolationLevel.ReadCommitted))
                //    {
                //        var batch = _campaignRepository.Table.Skip(i++ * batchSize).Take(batchSize);
                //        foreach (var campagin in batch)
                //        {
                //            if (string.IsNullOrWhiteSpace(campagin.Design) && campagin.CampaignDesign != null) continue;
                //            var campaign_design = new CampaignDesign()
                //            {
                //                Data = campagin.Design
                //            };
                //            _campaignDesign.Create(campaign_design);
                //            campagin.CampaignDesign = campaign_design;
                //            //campagin.Design = "";
                //            // _campaignRepository.Update(campagin);
                //            dics.Add(campaign_design.Id, campagin.Id);
                //        }
                //        //tx2.Commit();
                //        tx1.Commit();
                //        i++;
                //    }
                //}
            //}



        //    return 150;
        //}

        //public int UpdateFrom149()
        //{
        //    return 150;
        //}


        //public int UpdateFrom151()
        //{
        //    foreach (var campagin_id in _campaignRepository.Table.Select(aa => new { id = aa.Id }))
        //    {
        //        var campagin = _campaignRepository.Fetch(aa => aa.Id == campagin_id.id).First();
        //        campagin.Seller = (campagin.TeeyootUserId.HasValue) ? _userPartRepository.Get(campagin.TeeyootUserId.Value) : _userPartRepository.Get(0);
        //        _campaignRepository.Update(campagin);
        //    }
        //    return 148;
        //}

        //public int UpdateForm146()
        //{
        //    foreach (var order in _orderRepository.Table)
        //    {
        //        if (order.Products == null || order.Products.Count == 0) continue;

        //        foreach (var product in order.Products)
        //        {
        //            order.TotalSold += product.Count;
        //        }
        //        _orderRepository.Update(order);
        //    }
            
        //    return 147;
        //}

        //public int UpdateForm147()
        //{
        //    foreach (var campaign in _campaignRepository.Table)
        //    {
        //        campaign.ProductCountSold = 0;
        //        var orders = _orderRepository.Table.Where(aa => aa.Campaign.Id == campaign.Id);
        //        foreach (var order in orders)
        //        {
        //            if (order.OrderStatusRecord.Name == "Cancelled" || order.OrderStatusRecord.Name == "Pending" || order.OrderStatusRecord.Name == "Refunded") continue;
        //            campaign.ProductCountSold += order.TotalSold;
        //        }
        //        _campaignRepository.Update(campaign);
        //    }

        //    return 148;
        //}
        ////update campaign profit
        //public int UpdateFrom148()
        //{
            
            
        //    return 149;
        //}
    }
}
