
namespace Store.Data
{
    public class DataGlobals
    {
        //Данные перенесены в ApplicationConfig.xml
        //public const string ARM_ID_OZSMK = "403";
        //public const string ARM_NAME_OZSMK = "АРМ \"Спецодежда ОЗСМК\"";
        //public const string ARM_ID_EVRAZTECHNIKA = "553";
        //public const string ARM_NAME_EVRAZTECHNIKA = "АРМ \"Спецодежда ЕвразТехника\"";
        //public const string ARM_ID_VGOK = "563";
        //public const string ARM_NAME_VGOK = "АРМ \"Спецодежда ВГОК\"";

        public const string ORG_ID_VGOK = "3";
        public const string ORG_ID_UKU = "4";
        public const string ORG_ID_EVRAZRUDA = "5";
        public const string ORG_ID_GURYEVSK = "6";

        public const string ACCOUNT_KEY = "___account";
        public const string ACCOUNT_DB_FACTORY_KEY = "nhibernate.account_db";

        public const string DATE_FORMAT = "dd.MM.yy";
        public const string DATE_FORMAT_FULL_YEAR = "dd.MM.yyyy";
        public const string DATE_TIME_FORMAT = "dd.MM.yy HH:mm:ss";

        public const string STORAGE_NAME_ID_LIST = "storageNameId";

        public const string ROLE_ADMIN = "admin";
        public const string ROLE_VIEW_ALL = "viewAll";
        
        public const string ROLE_OPERATION_EDIT = "operationEdit";
        public const string ROLE_OPERATION_VIEW = "operationView";
        
        public const string ROLE_WORKER_EDIT = "workerEdit";
        public const string ROLE_WORKER_VIEW = "workerView";

        public const string ROLE_WORKER_CARD_EDIT = "workerCardEdit";
        public const string ROLE_WORKER_CARD_VIEW = "workerCardView";

        public const string ROLE_WORKER_CARD_OUT_EDIT = "workerCardOutEdit";
        public const string ROLE_WORKER_CARD_OUT_VIEW = "workerCardOutView";

        public const string ROLE_WORKER_CARD_RETURN_EDIT = "workerCardReturnEdit";
        public const string ROLE_WORKER_CARD_RETURN_VIEW = "workerCardReturnView";

        public const string ROLE_NOM_BODY_PART_EDIT = "nomBodyPartEdit";
        public const string ROLE_NOM_BODY_PART_VIEW = "nomBodyPartView";

        public const string ROLE_MOL_EDIT = "molEdit";
        public const string ROLE_MOL_VIEW = "molView";

        public const string ROLE_WORKER_SIZE_EDIT = "workerSizeEdit";
        public const string ROLE_WORKER_SIZE_VIEW = "workerSizeView";

        public const string ROLE_STORAGE_MONTH_EDIT = "storageMonthEdit";
        public const string ROLE_STORAGE_MONTH_VIEW = "storageMonthView";

        public const string ROLE_STORAGE_EDIT = "storageEdit";
        public const string ROLE_STORAGE_VIEW = "storageView";

        public const string ROLE_STORAGE_MOVE_OUT_EDIT = "moveStorageOutEdit";
        public const string ROLE_STORAGE_MOVE_OUT_VIEW = "moveStorageOutView";

        public const string ROLE_MESSAGE_EDIT = "messageEdit";

        public const string ROLE_MATPERSON_EDIT = "matPersonEdit";
        public const string ROLE_MATPERSON_VIEW = "matPersonEdit";

        public const string ROLE_OPER_TYPE_EDIT = "operTypeEdit";
        public const string ROLE_OPER_TYPE_VIEW = "operTypeView";

        public const string ROLE_ORGANIZATION_VIEW = "organizationView";
        public const string ROLE_ORGANIZATION_EDIT = "organizationEdit";

        public const string ROLE_TEST_REGISTER_VIEW = "testRegisterView";
        public const string ROLE_TEST_REGISTER_EDIT = "testRegisterEdit";


        public const string ROLE_AMSAP_LOAD = "AM_SAPLoad";
        public const string ROLE_NOMENCLATURE_EDIT = "nomenclatureEdit";
        public const string ROLE_NOMENCLATURE_VIEW = "nomenclatureView";

        public const string ROLE_NOMGROUP_EDIT = "nomGroupEdit";
        public const string ROLE_NOMGROUP_VIEW = "nomGroupView";

        public const string ROLE_STORNO_EDIT = "stornoEdit";
        public const string ROLE_STORNO_VIEW = "stornoView";
        
        public const string ROLE_NORMA_VIEW = "normaView";
        public const string ROLE_NORMA_EDIT = "normaEdit";
        public const string ROLE_NORMA_APPROVED = "normaApproved";

        public const string ROLE_CONFIG_EDIT = "configEdit";
        public const string ROLE_CONFIG_VIEW = "configView";

        public const string ROLE_REMAINS_EXTERNAL_UPLOAD = "remainsExternalEdit";
        public const string ROLE_REMAINS_EXTERNAL_VIEW = "remainsExternalView";
        // Экспорт данных для ВГОК в Галактику
        public const string ROLE_EXPORT_DATA = "exportData";
        public const string ROLE_INVENTORY = "loadInventory";
        //Табельные номера утверждающих в подразделениях личные карточки
        public const string ROLE_SUBSCRIPTION_VIEW = "subscriptionView";
        public const string ROLE_SUBSCRIPTION_EDIT = "subscriptionEdit";
        //Таблица комиссий для списания и подписантов документов
        public const string ROLE_SIGNDOCUMET_VIEW = "signdocumetView";
        public const string ROLE_SIGNDOCUMET_EDIT = "signdocumetEdit";


        public const string ROLE_REPORT_CIRCULATE = "reportCirculate";
        public const string ROLE_REPORT_PERSONAL_CARD = "reportPrsonalCard";
        public const string ROLE_REPORT_PLAN_PERSONAL = "reportPlanPrsonal";
        public const string ROLE_REPORT_PLAN_SHOP = "reportPlanShop";
        public const string ROLE_REPORT_REQUIREMENT = "reportRequirement";
        public const string ROLE_REPORT_NORMA_WORKPLACE = "reportNormaWorkPlace";
        public const string ROLE_REPORT_REQUIREMENT_VGOK = "reportRequirementVGOK";
        public const string ROLE_REPORT_PERSONAL_BY_SHOP = "reportPersonalByShop";
        public const string ROLE_REPORT_NOMENCLATURE = "reportNomenclature";
        public const string ROLE_REPORT_PROPUSK = "reportPropusk";
        public const string ROLE_REPORT_NOTTRANSFER = "reportNotTransfer";
        public const string ROLE_REPORT_AKTMB8 = "reportAktMb8";

        public const string ROLE_NOM_GROUP_PRICE_UPLOAD = "nomGroupPriceUpload";
        public const string ROLE_NOM_GROUP_PRICE_VIEW = "nomGroupPriceView";
        public const string ROLE_REPORT_USER_ACCESS = "reportUserAccess";
        public const string ROLE_REPORT_DUTY = "reportDuty";
        public const string ROLE_REPORT_SIZEMISMATCH = "reportSizeMismatch";

        // Наименование справочников для радактирования по шаблону
        public const string REFERENCE_UNIT = "Units";
        public const string REFERENCE_SEX = "Sexes";
        public const string REFERENCE_NOM_BODY_PART = "NomBodyParts";
        public const string REFERENCE_NOM_BODY_PART_SIZE_GROWTH = "NomBodyPartsGrowth";
        public const string REFERENCE_NOM_BODY_PART_SIZE = "NomBodyPartSizes";
        public const string REFERENCE_SHOP = "Shops";
        public const string REFERENCE_STORAGE = "Storages";
        public const string REFERENCE_RESULT = "Results";
        public const string REFERENCE_ENTERPRICE = "_EnterpriceList";
        public const string REFERENCE_STORAGE_OPERATION = "StoregeOperation";
        public const string REFERENCE_DOCUMENT_TYPE = "DocumenTypes";
        public const string REFERENCE_OPER_TYPE = "OperTypes";
        public const string REFERENCE_MOTIV = "Motivs";
        public const string REFERENCE_STORAGE_NAME = "StorageNames";
        public const string REFERENCE_STORAGE_SHOP_NAME = "StorageShopNames";
        public const string REFERENCE_NOMGROUPS = "NomGroups";
        public const string REFERENCE_MESSAGE = "UserMessages";
        public const string REFERENCE_PERIOD_PRICE = "PeriodPrice";
        public const string REFERENCE_SUBSCRIPTION = "Subscription";
        public const string REFERENCE_SIGNDOCTYPE = "SignDocTypes";
        public const string REFERENCE_SIGNTYPE = "SignTypes";
        // Типы операций
        public const int OPERATION_STORAGE_IN      = 1;        // приход на склад
        public const int OPERATION_STORAGE_OUT     = 2;        // списание со склада
        public const int OPERATION_WORKER_IN       = 3;        // выдача работнику
        public const int OPERATION_WORKER_OUT      = 4;        // списание с работника
        public const int OPERATION_WORKER_RETURN   = 5;        // возврат от работника
        public const int OPERATION_STORAGE_SAP_IN  = 6;        // приход на склад из SAP
        public const int OPERATION_STORAGE_WEAR_OUT = 7;       // Списание со склада б/у номенклатуры
        public const int OPERATION_WORKER_OUT_TIME = 8;        // Списание с работника по сроку
        public const int OPERATION_STORAGE_SAP_OUT = 11;       // Расход со склада в цех
        public const int OPERATION_STORAGE_TRANSFER_OUT = 9;   // Перевод. Списание с забаланса
        public const int OPERATION_STORAGE_TRANSFER_IN = 10;   // Перевод. Постановка на забаланс
        public const int OPERATION_MOL_STORAGE_IN = 13;        //Возврат дежурной на склад
        public const int OPERATION_MOL_STORAGE_OUT = 11;       //Выдача дежурной МОЛ со склада
        public const int OPERATION_MOL_OUT = 12;               //Списание дежурной с МОЛ
        public const int OPERATION_MOL_WORKER_IN = 14;        //Выдача дежурной от МОЛ работнику
        public const int OPERATION_MOL_WORKER_RETURN = 15;               //Возврат дежурной от работника МОЛ
        public const int OPERATION_MOL_MOVE_OUT = 16;          //Перевод дежурной. Списание с забаланса
        public const int OPERATION_MOL_MOVE_IN = 17;           //Перевод дежурной. Постановка на забаланс
        
        public const int MOTIVID_TRANSFER = 14;                 // Причина постановки на забаланс Инсорсинг

        // Причина досрочного списания спец. одежды 
        public const int CAUSE_OPERATION_WORKER_OUT = 1; // Производственные причины
        //Вид размера
        public const int GROWTH_SIZE_ID = 1;        // Код типа размера Рост
        public const int CLOTH_SIZE_ID = 2;         // Код типа размера Одежда
        public const int SIZ_SIZE_ID = 10;          // Код типа размера СИЗ (без размера)
        //Исключение из проверки на ввод размера при вводе приходного ордера
        public const int GLOVE_SIZE_ID = 5;          // Код типа размера Перчатки
        public const int HAND_SIZE_ID = 4;          // Код типа размера Рукавицы 
        public const int RESP_SIZE_ID = 6;          // Код типа размера Респиратор
        public const int UNISEX_ID = 3;             // ID SEX пол унисекс

        //Виды документов
        public const int DOCTYPE_TTN = 3;

        public const string EXCLUDE_ID_INCOMING_STORAGE_DOCTYPE = "2,4,5";
        //Идентификаторы СИЗ, которые баз размера
        public const string EXCLUDE_ID_SIZ = "10";

        // кол-во дней от текущей даты, для периода возможных операций
        public const int MIN_DATE_OPER = 45;
        //Код подразделения Дивизион Сибирь, относящийся к ЗСМК
        public const string ZSMK_DIVIZION = "E850";
        //Код подразделения Дивизион Сибирь, относящийся к Евразруде
        public const string EVRAZRUDA_DIVIZION = "E490";
        // МВЗ для дивизиона Сибирь в Евразруде
        //При переходе на SAP поменять МВЗ на 8020010026
        public const string MVZ_EVRAZRUDA_DIVIZION = "8020060005" /* "11227"*/;

        //При присоединении Евразруды и частти дивизиона
        public const string MVZ_ZSMK_RUDA_DIVIZION = "2660100090" ;
    }

}
