using System.Web.Mvc;
using SharpArch.Core;
using SharpArch.Web.NHibernate;
using Store.Core;
using Telerik.Web.Mvc;
using System.Collections.Generic;
using Store.Data;
using System;
using System.Text;

namespace Store.Web.Controllers
{
    [HandleError]
    public class MatPersonStoreController : ViewedController
    {
        private readonly CriteriaRepository<MatPersonCardHead> matPersonCardHeadRepository;
        private readonly CriteriaRepository<MatPersonCardContent> matPersonCardContentRepository;
        private readonly CriteriaRepository<MatPersonOnHands> matPersonOnHandsRepository;
        private readonly CriteriaRepository<Worker> workerRepository;
        private readonly CriteriaRepository<WorkerCardHead> workerCardHeadRepository;
        private readonly StorageNameRepository storageNameRepository;
        private readonly StorageRepository storageRepository;
        private readonly OrganizationRepository organizationRepository;
        private readonly CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository;
        private readonly CriteriaRepository<Motiv> motivRepository;
        private readonly CriteriaRepository<OperType> operTypeRepository;
        private readonly OperationRepository operationRepository;
        private readonly CriteriaRepository<WorkerCardContent> workerCardContentRepository;
        private readonly CriteriaRepository<NormaContent> normaContentRepository;
        private readonly CriteriaRepository<NormaOrganization> normaOrganizationRepository;
        private readonly CriteriaRepository<WorkerCardHead> workerCardRepository;
        private readonly CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository;
        private readonly CriteriaRepository<Sex> sexRepository;
        private readonly static IFormatProvider culture = new System.Globalization.CultureInfo("ru-RU", true);

        public MatPersonStoreController(
            CriteriaRepository<MatPersonCardHead> matPersonCardHeadRepository,
            CriteriaRepository<Worker> workerRepository,
            StorageNameRepository storageNameRepository,
            OrganizationRepository organizationRepository,
            CriteriaRepository<WorkerWorkplace> workerWorkplaceRepository,
            CriteriaRepository<MatPersonCardContent> matPersonCardContentRepository,
            CriteriaRepository<MatPersonOnHands> matPersonOnHandsRepository,
            StorageRepository storageRepository,
            CriteriaRepository<WorkerCardHead> workerCardHeadRepository,
            CriteriaRepository<Motiv> motivRepository,
            CriteriaRepository<OperType> operTypeRepository,
            OperationRepository operationRepository,
            CriteriaRepository<WorkerCardContent> workerCardContentRepository,
            CriteriaRepository<NormaContent> normaContentRepository,
            CriteriaRepository<NormaOrganization> normaOrganizationRepository,
            CriteriaRepository<WorkerCardHead> workerCardRepository,
            CriteriaRepository<NomBodyPartSize> nomBodyPartSizeRepository,
            CriteriaRepository<Sex> sexRepository
            )
        {
            Check.Require(matPersonOnHandsRepository != null, "matPersonOnHandsRepository may not be null");
            this.matPersonCardHeadRepository = matPersonCardHeadRepository;
            this.workerRepository = workerRepository;
            this.organizationRepository = organizationRepository;
            this.workerWorkplaceRepository = workerWorkplaceRepository;
            this.storageNameRepository = storageNameRepository;
            this.matPersonCardContentRepository = matPersonCardContentRepository;
            this.matPersonOnHandsRepository = matPersonOnHandsRepository;
            this.storageRepository = storageRepository;
            this.operationRepository = operationRepository;
            this.workerCardHeadRepository = workerCardHeadRepository;
            this.motivRepository = motivRepository;
            this.operTypeRepository = operTypeRepository;
            this.workerCardContentRepository = workerCardContentRepository;
            this.normaContentRepository = normaContentRepository;
            this.normaOrganizationRepository = normaOrganizationRepository;
            this.workerCardRepository = workerCardRepository;
            this.nomBodyPartSizeRepository = nomBodyPartSizeRepository;
            this.sexRepository = sexRepository;
        }

        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_MOL_EDIT + ", " + DataGlobals.ROLE_MOL_VIEW))]
        public ActionResult Index()
        {
            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("Organization.Id", int.Parse(getCurrentEnterpriseId()));
            query.Add("[in]Id", ((Store.Core.Account.User)Session[DataGlobals.ACCOUNT_KEY]).ObjectsCSV);
            IList<StorageName> storageNames = storageNameRepository.GetByCriteria(query);
            SelectList storageNameList = new SelectList(storageNames, "Id", "Name", Session["storageNameId"] != null ? (string)Session["storageNameId"] : "-1");
            ViewData[DataGlobals.REFERENCE_STORAGE_SHOP_NAME] = storageNameList;
            ViewData["isMOL"] = false;
            query.Clear();
            query.Add("OperType.Id", 12);
            IList<Motiv> motivs = motivRepository.GetByCriteria(query);
            ViewData[DataGlobals.REFERENCE_MOTIV] = motivs;

            return View(viewName);
        }

        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_MOL_EDIT + ", " + DataGlobals.ROLE_MOL_VIEW))]
        public ActionResult _SelectOnHands(int workerWorkPlaceId)
        {
            MatPersonCardHead matperson = matPersonCardHeadRepository.Get(workerWorkPlaceId);
            List<MatPersonOnHands> model = new List<MatPersonOnHands>();
            if (matperson != null)
            {
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                queryParams.Add("MatPersonCardHead", matperson);
                queryParams.Add("[>]Quantity", 0);
                IList<MatPersonOnHands> onHands = matPersonOnHandsRepository.GetByLikeCriteria(queryParams);
                List<string> excludeProperty = new List<string>();
                excludeProperty.Add("MatPersonCardHead");
                foreach (var item in onHands)
                {
                    MatPersonOnHands resItem = rebuildMatPersonOnHand(item, excludeProperty);
                    model.Add(resItem);
                };
            }
            return View(new GridModel(model));
        }
//Поиск номенклатур на складе к которому относится МОЛ или номенклатур на руках у МОЛ
        //mode=0 на складе
        //mode=1 на руках
        //mode=4 на руках по норме сотрудника

        [HttpPost]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_MOL_EDIT + ", " + DataGlobals.ROLE_MOL_VIEW))]
        public ActionResult _GetNomenclaturesOnStorage(int mode, string text, int idMol, int normaContentId)
        {
            MatPersonCardHead matperson = matPersonCardHeadRepository.Get(idMol);

            Dictionary<string, object> query = new Dictionary<string, object>();

            if (mode == 0)
            {
                IList<Storage> storages = new List<Storage>();

                int code = -1;
                if (text.Length>2 && int.TryParse(text.Substring(2), out code))
                    query.Add("Nomenclature.ExternalCode", text);
                else
                {
                    if (text.Length <= 2) text = "";
                    query.Add("Nomenclature.Name", text);
                }
                query.Add("StorageName", matperson.StorageName);
                query.Add("[>]Quantity", 0);
                query.Add("[!=]Wear", "50");
                storages = storageRepository.GetByLikeCriteria(query);
                return new JsonResult
                {
                    Data = new SelectList(storages, "Id", "StorageInfo")
                };

            };
            if (mode == 1) {
                IList<MatPersonOnHands> nomenclatures = new List<MatPersonOnHands>();
                query.Add("MatPersonCardHead", matperson);
                query.Add("[>]Quantity", 0); 
                nomenclatures = matPersonOnHandsRepository.GetByLikeCriteria(query);
                return new JsonResult
                {
                    Data = new SelectList(nomenclatures, "Nomenclature.Id", "NomenclatureInfo")
                };
            }
            if (mode == 4)
            {

                Dictionary<string, object> order = new Dictionary<string, object>();
                IList<MatPersonOnHands> nomenclatures = new List<MatPersonOnHands>();
                query.Add("MatPersonCardHead", matperson);
                int code = -1;
                if (text.Length > 2 && int.TryParse(text.Substring(2), out code))
                    query.Add("Nomenclature.ExternalCode", text);
                else
                {
                    if (text.Length <= 2) text = "";
                    query.Add("Nomenclature.Name", text);
                }
                query.Add("[>]Quantity", 0);

                query.Add("StorageName", matperson.StorageName);
                NormaContent normaContent = normaContentRepository.Get(normaContentId);

                // сортировка по размеру, но если не будет размера у номенклатуры, то непопадет в выборку
                //if (normaContent.NomGroup.NomBodyPart != null && DataGlobals.SIZ_SIZE_ID != normaContent.NomGroup.NomBodyPart.Id)
                //{
                //    order.Add("Nomenclature.Sex.Id", ASC);
                //    order.Add("NomBodyPartSize.SizeNumber", ASC);
                //}

                StringBuilder nomGroupBase = new StringBuilder();
                StringBuilder nomGroups = new StringBuilder();
                foreach (var normaNomGroup in normaContent.NormaNomGroups)
                {
//Выбираем и основную группы и группу замены
//                    if (normaNomGroup.IsBase == true)
//                    {
                        nomGroupBase.Append("'" + normaNomGroup.NomGroup.Id + "',");
                    //    break;
                    //}
                }
                nomGroupBase.Remove(nomGroupBase.Length - 1, 1);

                query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());
                order.Add("Wear", DESC);
                order.Add("Nomenclature.Id", ASC);

                nomenclatures = matPersonOnHandsRepository.GetByLikeCriteria(query, order);
                return new JsonResult
                {
                    Data = new SelectList(nomenclatures, "IdNomenclatureAndWear", "NomenclatureInfo")
                };

            }
            return null;
        }
     
//Поиск МОЛ
        [HttpPost]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_MOL_EDIT + ", " + DataGlobals.ROLE_MOL_VIEW))]
        public ActionResult _FindMatPerson(string text)
        {
            Organization currentOrg = organizationRepository.Get(getIntCurrentEnterpriseId());

            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> orderParams = new Dictionary<string, object>();

            IList<MatPersonCardHead> workers = null;
            int tabn = -1;
            if (int.TryParse(text, out tabn))
                queryParams.Add("Worker.TabN", tabn);
            else
                queryParams.Add("Worker.Fio", text);

            queryParams.Add("Organization", currentOrg);
            queryParams.Add("IsActive", true);
            queryParams.Add("[in]StorageName.Id", ((Store.Core.Account.User)Session[DataGlobals.ACCOUNT_KEY]).ObjectsCSV);            
            orderParams.Add("Worker.Fio", ASC);
            workers = matPersonCardHeadRepository.GetByLikeCriteria(queryParams, orderParams);
                        
            return new JsonResult
            {
//                Data = new SelectList(workers, "MatPersonIdAndStorageNameId", "MatPersonInfo")
                Data = new SelectList(workers, "MatPersonIdAndStorageNameId", "MatPersonFullInfo")
            };
        }

        // Поиск сотрудника по рабочему месту
        [HttpPost]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_MOL_EDIT + ", " + DataGlobals.ROLE_MOL_VIEW))]
        public ActionResult _FindActiveWorkerWorkPlace(string text)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> orderParams = new Dictionary<string, object>();

            IList<WorkerWorkplace> workerWorkplaces = null;
            int tabn = -1;
            if (int.TryParse(text, out tabn))
                queryParams.Add("Worker.TabN", tabn);
            else
                queryParams.Add("Worker.Fio", text);
            queryParams.Add("RootOrganization", int.Parse(getCurrentEnterpriseId()));
            queryParams.Add("IsActive", true);
            orderParams.Add("Worker.Fio", ASC);

            workerWorkplaces = workerWorkplaceRepository.GetByLikeCriteria(queryParams, orderParams);

            return new JsonResult
            {
                Data = new SelectList(workerWorkplaces, "Id", "WorkplaceInfo")
            };
        }

        // Поиск сотрудника по рабочему месту
        [HttpPost]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_MOL_EDIT + ", " + DataGlobals.ROLE_MOL_VIEW))]
        public ActionResult _FindWorkerWorkPlace(string text)
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            Dictionary<string, object> orderParams = new Dictionary<string, object>();

            IList<WorkerWorkplace> workerWorkplaces = null;
            int tabn = -1;
            if (int.TryParse(text, out tabn))
                queryParams.Add("Worker.TabN", tabn);
            else
                queryParams.Add("Worker.Fio", text);
            queryParams.Add("RootOrganization", int.Parse(getCurrentEnterpriseId()));
            orderParams.Add("Worker.Fio", ASC);

            workerWorkplaces = workerWorkplaceRepository.GetByLikeCriteria(queryParams, orderParams);

            return new JsonResult
            {
                Data = new SelectList(workerWorkplaces, "Id", "WorkplaceInfo")
            };
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_MOL_EDIT + ", " + DataGlobals.ROLE_MOL_VIEW))]
        public ActionResult _SelectNomenclatureOnHands(int workerWorkplaceId) 
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            queryParams.Add("WorkerWorkplace.Id", workerWorkplaceId);
            WorkerCardHead wch = workerCardHeadRepository.FindOne(queryParams);
            IList<MatPersonOnHandsSimple> list = new List<MatPersonOnHandsSimple>();
            if (wch != null) {
                var items = wch.WorkerCardContents;
                foreach (var item in items){
                    //Если номенклатурва на руках
                    if (item.Quantity > 0)
                    {
                        var itm = new MatPersonOnHandsSimple(item.Id);
                        itm.Nomenclature = item.Storage.Nomenclature.Name;
                        itm.ExternalCode = item.Storage.Nomenclature.ExternalCode;
                        itm.NomenclatureId = item.Id;
                        itm.Wear = item.Storage.Wear; 
                        itm.Quantity = item.Quantity;
                        list.Add(itm);
                    }
                }
            }

            return View(new GridModel(list));       
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_MOL_EDIT))]
        //Возврат от сотрудника
        public ActionResult _outPersonSave(int toMatPersonId, int workerWorkPlaceId, string docDate, IEnumerable<MatPersonOnHandsSimple> updated)
        {
            MatPersonCardHead toMatperson = matPersonCardHeadRepository.Get(toMatPersonId);
            DateTime date = DateTime.ParseExact(docDate, DataGlobals.DATE_FORMAT_FULL_YEAR, culture);
            date = date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            WorkerWorkplace wwp = workerWorkplaceRepository.Get(workerWorkPlaceId);
            orderParams.Add("Quantity", DESC);
            if ((toMatperson != null) && (updated != null))
            {
                operationRepository.DbContext.BeginTransaction();
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                string operationDocNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_WORKER_RETURN, toMatperson.Organization.Id.ToString());


                foreach (var item in updated)
                {
                    if (item.WorkQuantity <= 0)
                    {
                        continue;
                    }
                    try
                    {
                        //Ищем номенклатуру у Сотрудника
                        WorkerCardContent wcc = workerCardContentRepository.Get(item.NomenclatureId);
                        if (wcc == null) {
                            ModelState.AddModelError("", "У сотрудника не найдена на руках номенклатура `" + item.Nomenclature + "`.");
                            continue;
                        }

                        Storage storage = wcc.Storage;
                        if (storage == null)
                        {
                            ModelState.AddModelError("", "Номенклатура `" + item.Nomenclature + "` на складе не найдена.");
                            continue;
                        }
                        if (toMatperson.StorageName.Id != storage.StorageName.Id)
                        {
                            ModelState.AddModelError("", "МОЛ может принять номенклатуры только со своего склада.");
                            continue;
                        }

                            if (item.WorkQuantity > wcc.Quantity)
                            {
                                ModelState.AddModelError("", "У сотрудника не хватает номенклатуры `" + item.Nomenclature + "`.");
                                continue;
                            }
                            //--------------МОЛ--------------
                            //Создаем операцию возврата
                            Operation operation = new Operation();
                            operation.Organization = toMatperson.Organization;
                            operation.Quantity = item.WorkQuantity;
                            operation.StorageName = toMatperson.StorageName;
                            operation.Wear = item.Wear;
                            operation.Storage = storage;
                            operation.DocNumber = operationDocNumber;
                            operation.OperDate = date;
                            operation.DocDate = date;
                            operation.WorkerWorkplace = wcc.WorkerCardHead.WorkerWorkplace;
                            operation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_MOL_WORKER_RETURN);

                            //--------------Сохраняем данные--------------
                            operationRepository.SaveOrUpdate(operation);

                            //Создаем содержимое для МОЛ
                            MatPersonCardContent mpc = new MatPersonCardContent();
                            mpc.MatPersonCardHead = toMatperson;
                            mpc.Quantity = item.WorkQuantity;
                            mpc.Storage = storage;
                            mpc.OperDate = date;
                            mpc.Wear = item.Wear;
                            mpc.Operation = operation;
                            mpc.OperType = operation.OperType;
                            //--------------Сотрудник--------------

                            if (item.WorkQuantity == wcc.Quantity)
                            {
                                wcc.OperReturn = operation;
                                wcc.Quantity = 0;
                                wcc.EndDate = operation.OperDate;
                            }
                            else
                            {
                                wcc.Quantity = wcc.Quantity - item.WorkQuantity;
                                WorkerCardContent wccNew = new WorkerCardContent();
                                wccNew.Quantity = 0;
                                wccNew.StartDate = wcc.StartDate;
                                wccNew.EndDate = operation.OperDate;
                                wccNew.WorkerCardHead = wcc.WorkerCardHead;
                                wccNew.Storage = wcc.Storage;
                                wccNew.Operation = wcc.Operation;
                                wccNew.OperReturn = operation;
                                wccNew.NormaContent = wcc.NormaContent;
                                workerCardContentRepository.SaveOrUpdate(wccNew);
                            }

                            //--------------Сохраняем данные--------------

                            matPersonCardContentRepository.SaveOrUpdate(mpc);
                            workerCardContentRepository.SaveOrUpdate(wcc);
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", "При сохранении данных по номеклатуре `" + item.Nomenclature + "` произошла ошибка.\n" + e.Message);
                    }
                }

                if (ModelState.IsValid)
                {
                    operationRepository.DbContext.CommitTransaction();
                }
                else
                {
                    operationRepository.DbContext.RollbackTransaction();
                }

            }
            return View(new GridModel(new List<MatPersonOnHandsSimple>()));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_MOL_EDIT))]
        //Выдача сотруднику от МОЛ
        public ActionResult _inPersonSave(int fromMatPersonId, int workerWorkPlaceId, string docDate, IEnumerable<WorkerNorma> updated)
        {
            MatPersonCardHead fromMatperson = matPersonCardHeadRepository.Get(fromMatPersonId);
            WorkerWorkplace workerWorkplace = workerWorkplaceRepository.Get(workerWorkPlaceId);
            DateTime date = DateTime.ParseExact(docDate, DataGlobals.DATE_FORMAT_FULL_YEAR, culture);
            date = date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);

            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("Quantity", DESC);

            if ((fromMatperson != null) && (updated != null))
            {
                operationRepository.DbContext.BeginTransaction();
                string operationDocNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_WORKER_IN, fromMatperson.Organization.Id.ToString());
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                foreach (var item in updated)
                {
                    if (item.PutQuantity <= 0)
                    {
                        continue;
                    }
                    //На руках уже усть номенклатура в достаточном кол-ве
                    if (item.NormaQuantity-item.PresentQuantity <= 0)
                    {
                        continue;
                    }
                    queryParams.Clear();
                    queryParams.Add("MatPersonCardHead", fromMatperson);
                    queryParams.Add("Nomenclature.Id", item.StorageId);
                    queryParams.Add("Wear", item.Wear);
                    queryParams.Add("[>]Quantity", 0);
                    IList<MatPersonOnHands> list = matPersonOnHandsRepository.GetByLikeCriteria(queryParams);
                    if (list.Count == 0) {
                        ModelState.AddModelError("", "Номенклатура `" + item.StorageInfo + "` у МОЛ не найдена.");
                        continue;
                    }

                    if (item.PutQuantity > list[0].Quantity)
                    {
                        ModelState.AddModelError("", "У МОЛ не хватает номенклатуры `" + item.StorageInfo + "`.");
                        continue;
                    }

                    try
                    {
                        //Ищем номенклатуру на складе
                        queryParams.Clear();
                        queryParams.Add("Wear", item.Wear);
                        queryParams.Add("StorageName", fromMatperson.StorageName);
                        queryParams.Add("Nomenclature.Id", item.StorageId);
                        Storage storage = null;
                        IList<Storage> lists = storageRepository.GetByLikeCriteria(queryParams, orderParams);
                        if (lists.Count > 0)
                        {
                            storage = lists[0];
                        }
                        if (storage == null)
                        {
                            ModelState.AddModelError("", "Номенклатура `" + item.StorageInfo + "` на складе не найдена.");
                        }
                        else
                        {
                          //  storage.Quantity = storage.Quantity + item.WorkQuantity;

                            //Создаем операцию Выдача одежды сотруднику от МОЛ
                            Operation operation = new Operation();
                            operation.Organization = fromMatperson.Organization;
                            operation.Quantity = item.PutQuantity;
                            operation.StorageName = fromMatperson.StorageName;
                            operation.Wear = storage.Wear;
                            operation.Storage = storage;
                            operation.DocNumber = operationDocNumber;
                            operation.OperDate = date;
                            operation.DocDate = date;
                            operation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_MOL_WORKER_IN);
                            operation.IsCorporate = item.IsCorporate;
                            operation.WorkerWorkplace = workerWorkplace;

                            //Создаем содержимое для МОЛ
                            MatPersonCardContent mpc = new MatPersonCardContent();
                            mpc.MatPersonCardHead = fromMatperson;
                            mpc.Quantity = -1 * item.PutQuantity;
                            mpc.Storage = storage;
                            mpc.OperDate = date;
                            mpc.Wear = item.Wear;
                            mpc.Operation = operation;
                            mpc.OperType = operation.OperType;

                            operationRepository.SaveOrUpdate(operation);
                            matPersonCardContentRepository.SaveOrUpdate(mpc);

//Операции с личной карточкой
                            queryParams.Clear();
                            queryParams.Add("WorkerWorkplace", workerWorkplace);

                            WorkerCardHead workerCardHead = workerCardRepository.FindOne(queryParams);
                            if (workerCardHead == null)
                            {
                                workerCardHead = new WorkerCardHead();
                                workerCardHead.WorkerCardContents = new List<WorkerCardContent>();
                                workerCardHead.WorkerWorkplace = workerWorkplace;
                            }
                            // ищем на руках номенклатуру по текущей группе нормы
                            //WorkerCardContent workerCardContentPresent = getWorkerCardPresent(normaContent.NormaNomGroups, workerCardHead.WorkerCardContents);
                            WorkerCardContent workerCardContent = null;

                            // если есть, то используем эту же позицию карточки
                            //if (workerCardContentPresent != null)
                            //    workerCardContent = new WorkerCardContent(workerCardContentPresent.Id);
                            //else
                            workerCardContent = new WorkerCardContent();

                            //storage.Quantity -= item.PutQuantity;


                            workerCardContent.Storage = storage;
                            workerCardContent.Quantity = item.PutQuantity;
                            workerCardContent.Operation = operation;
                            workerCardContent.NormaContent = normaContentRepository.Get(item.NormaContentId);
                            workerCardContent.WorkerCardHead = workerCardHead;
                            workerCardContent.StartDate = operation.OperDate;
                            workerCardContent.IsCorporate = item.IsCorporate;
                            workerCardHead.WorkerCardContents.Add(workerCardContent);

                            workerCardRepository.SaveOrUpdate(workerCardHead);
                        }
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", "При сохранении данных по номеклатуре `" + item.StorageInfo + "` произошла ошибка.\n" + e.Message);
                    }
                }

                if (ModelState.IsValid)
                {
                    operationRepository.DbContext.CommitTransaction();
                }
                else
                {
                    operationRepository.DbContext.RollbackTransaction();
                }
            }
            


            return View(new GridModel(new List<WorkerNorma>()));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_MOL_EDIT))]
        //Перемещение между МОЛ
        public ActionResult _moveMolSave(int fromMatPersonId, int toMatPersonId, string docDate, IEnumerable<MatPersonOnHandsSimple> updated)
        {
            DateTime date = DateTime.ParseExact(docDate, DataGlobals.DATE_FORMAT_FULL_YEAR, culture);
            date = date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);

            MatPersonCardHead fromMatperson = matPersonCardHeadRepository.Get(fromMatPersonId);
            MatPersonCardHead toMatperson = matPersonCardHeadRepository.Get(toMatPersonId);
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("Quantity", DESC);
            if ((fromMatperson != null) && (toMatperson != null) && (updated != null))
            {
                if (fromMatperson.Id != toMatperson.Id)
                {
                    operationRepository.DbContext.BeginTransaction();
                    string operationDocNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_STORAGE_TRANSFER_OUT, fromMatperson.Organization.Id.ToString());
                    Dictionary<string, object> queryParams = new Dictionary<string, object>();
                    foreach (var item in updated)
                    {
                        if (item.WorkQuantity <= 0)
                        {
                            continue;
                        }
                        try
                        {
                            //Ищем номенклатуру у МОЛ
                            queryParams.Clear();
                            queryParams.Add("Wear", item.Wear);
                            queryParams.Add("StorageName", fromMatperson.StorageName);
                            queryParams.Add("Nomenclature.Id", item.NomenclatureId);
                            Storage storage = null;
                            IList<Storage> list = storageRepository.GetByLikeCriteria(queryParams, orderParams);
                            if (list.Count > 0)
                            {
                                storage = list[0];
                            }

                            if (storage == null)
                            {
                                ModelState.AddModelError("", "Номенклатура `" + item.Nomenclature + "` у МОЛ не найдена.");
                            }
                            else
                            {
                                if (item.WorkQuantity > item.Quantity)
                                {
                                    ModelState.AddModelError("", "У МОЛ не хватает номенклатуры `" + item.Nomenclature + "`.");
                                    continue;
                                }

                                //Создаем операцию перемещения с МОЛ
                                Operation operationFrom = new Operation();
                                operationFrom.Organization = fromMatperson.Organization;
                                operationFrom.Quantity = item.WorkQuantity;
                                operationFrom.StorageName = fromMatperson.StorageName;
                                operationFrom.Wear = item.Wear;
                                operationFrom.Storage = storage;
                                operationFrom.DocNumber = operationDocNumber;
                                operationFrom.OperDate = date;
                                operationFrom.DocDate = date;
                                operationFrom.OperType = operTypeRepository.Get(DataGlobals.OPERATION_MOL_MOVE_OUT);

                                //Создаем содержимое для МОЛ
                                MatPersonCardContent mpcFrom = new MatPersonCardContent();
                                mpcFrom.MatPersonCardHead = fromMatperson;
                                mpcFrom.Quantity = -1 * item.WorkQuantity;
                                mpcFrom.Storage = storage;
                                mpcFrom.OperDate = date;
                                mpcFrom.Wear = item.Wear;
                                mpcFrom.Operation = operationFrom;
                                mpcFrom.OperType = operationFrom.OperType;

                                //Создаем операцию перемещения с МОЛ
                                Operation operationTo = new Operation();
                                operationTo.Organization = fromMatperson.Organization;
                                operationTo.Quantity = item.WorkQuantity;
                                operationTo.StorageName = fromMatperson.StorageName;
                                operationTo.Wear = item.Wear;
                                operationTo.Storage = storage;
                                operationTo.DocNumber = operationDocNumber;
                                operationTo.OperDate = date;
                                operationTo.DocDate = date;
                                operationTo.OperType = operTypeRepository.Get(DataGlobals.OPERATION_MOL_MOVE_IN);

                                operationTo.TransferOperation = operationFrom;
                                operationFrom.TransferOperation = operationTo;

                                //Создаем содержимое для МОЛ
                                MatPersonCardContent mpcTo = new MatPersonCardContent();
                                mpcTo.MatPersonCardHead = toMatperson;
                                mpcTo.Quantity = item.WorkQuantity;
                                mpcTo.Storage = storage;
                                mpcTo.OperDate = date;
                                mpcTo.Wear = item.Wear;
                                mpcTo.Operation = operationTo;
                                mpcTo.OperType = operationTo.OperType;

                                operationRepository.SaveOrUpdate(operationFrom);
                                matPersonCardContentRepository.SaveOrUpdate(mpcFrom);
                                operationRepository.SaveOrUpdate(operationTo);
                                matPersonCardContentRepository.SaveOrUpdate(mpcTo);
                            }
                        }
                        catch (Exception e)
                        {
                            ModelState.AddModelError("", "При сохранении данных по номеклатуре `" + item.Nomenclature + "` произошла ошибка.\n" + e.Message);
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        operationRepository.DbContext.CommitTransaction();
                    }
                    else
                    {
                        operationRepository.DbContext.RollbackTransaction();
                    }

                }
                else {
                    ModelState.AddModelError("", "Передать номенклатуры самому себе нельзя.");
                }
            }
            return View(new GridModel(new List<MatPersonOnHandsSimple>()));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_MOL_EDIT))]
        // Списание с МОЛ
        public ActionResult _outMolSave(int cause, int fromMatPersonId,string docDate, IEnumerable<MatPersonOnHandsSimple> updated)
        {
            DateTime date = DateTime.ParseExact(docDate, DataGlobals.DATE_FORMAT_FULL_YEAR, culture);
            date = date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);
            MatPersonCardHead fromMatperson = matPersonCardHeadRepository.Get(fromMatPersonId);
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            Motiv motiv = motivRepository.Get(cause);
            orderParams.Add("Quantity", DESC);
            if ((fromMatperson != null) && (updated != null))
            {
                operationRepository.DbContext.BeginTransaction();
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                string operationDocNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_MOL_OUT, fromMatperson.Organization.Id.ToString());
                foreach (var item in updated)
                {
                    if (item.WorkQuantity <= 0)
                    {
                        continue;
                    }
                    try
                    {
                        //Ищем номенклатуру у МОЛ
                        queryParams.Clear();
                        queryParams.Add("Wear", item.Wear);
                        queryParams.Add("StorageName", fromMatperson.StorageName);
                        queryParams.Add("Nomenclature.Id", item.NomenclatureId);
                        Storage storage = null;
                        IList<Storage> list = storageRepository.GetByLikeCriteria(queryParams, orderParams);
                        if (list.Count > 0)
                        {
                            storage = list[0];
                        }

                        if (storage == null)
                        {
                            ModelState.AddModelError("", "Номенклатура `" + item.Nomenclature + "` у МОЛ не найдена.");
                        }
                        else
                        {
                            if (item.WorkQuantity > item.Quantity)
                            {
                                ModelState.AddModelError("", "У МОЛ не хватает номенклатуры `" + item.Nomenclature + "`.");
                                continue;
                            }

                            //Создаем операцию перемещения
                            Operation operation = new Operation();
                            operation.Organization = fromMatperson.Organization;
                            operation.Quantity = item.WorkQuantity;
                            operation.StorageName = fromMatperson.StorageName;
                            operation.Wear = item.Wear;
                            operation.Storage = storage;
                            operation.DocNumber = operationDocNumber;
                            operation.OperDate = date;
                            operation.DocDate = date;
                            operation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_MOL_OUT);
                            operation.Motiv =  motiv;

                            //Создаем содержимое для МОЛ
                            MatPersonCardContent mpc = new MatPersonCardContent();
                            mpc.MatPersonCardHead = fromMatperson;
                            mpc.Quantity = -1 * item.WorkQuantity;
                            mpc.Storage = storage;
                            mpc.OperDate = date;
                            mpc.Wear = item.Wear;
                            mpc.Operation = operation;
                            mpc.OperType = operation.OperType;

                            operationRepository.SaveOrUpdate(operation);
                            matPersonCardContentRepository.SaveOrUpdate(mpc);
                        }
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", "При сохранении данных по номеклатуре `" + item.Nomenclature + "` произошла ошибка.\n" + e.Message);
                    }
                }

                if (ModelState.IsValid)
                {
                    operationRepository.DbContext.CommitTransaction();
                }
                else
                {
                    operationRepository.DbContext.RollbackTransaction();
                }

            }
            return View(new GridModel(new List<MatPersonOnHandsSimple>()));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_MOL_EDIT))]
        //Приход на склад от МОЛ
        public ActionResult _inStorageSave(int fromMatPersonId, string docDate, IEnumerable<MatPersonOnHandsSimple> updated)
        {
            DateTime date = DateTime.ParseExact(docDate, DataGlobals.DATE_FORMAT_FULL_YEAR, culture);
            date = date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);
            MatPersonCardHead fromMatperson = matPersonCardHeadRepository.Get(fromMatPersonId);
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("Quantity", DESC);
            if ((fromMatperson != null) && (updated != null))
            {

                operationRepository.DbContext.BeginTransaction();
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                string operationDocNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_WORKER_RETURN, fromMatperson.Organization.Id.ToString());
                foreach (var item in updated)
                {
                    if (item.WorkQuantity <= 0)
                    {
                        continue;
                    }

                    if (item.WorkQuantity > item.Quantity)
                    {
                        ModelState.AddModelError("", "У МОЛ нет номенклатура `" + item.Nomenclature + "` в таком кол-ве.");
                        continue;
                    }
                    try
                    {
                        //Ищем номенклатуру на складе
                        queryParams.Clear();
                        queryParams.Add("Wear", item.Wear);
                        queryParams.Add("StorageName", fromMatperson.StorageName);
                        queryParams.Add("Nomenclature.Id", item.NomenclatureId);
                        Storage storage = null;
                        IList<Storage> list = storageRepository.GetByLikeCriteria(queryParams,orderParams);
                        if (list.Count > 0) {
                            storage = list[0];
                        }
                        if (storage == null)
                        {
                            ModelState.AddModelError("", "Номенклатура `" + item.Nomenclature + "` на складе не найдена.");
                        }
                        else
                        {
                            storage.Quantity = storage.Quantity + item.WorkQuantity;

                            //Создаем операцию перемещения
                            Operation operation = new Operation();
                            operation.Organization = fromMatperson.Organization;
                            operation.Quantity = item.WorkQuantity;
                            operation.StorageName = fromMatperson.StorageName;
                            operation.Wear = item.Wear;
                            operation.Storage = storage;
                            operation.DocNumber = operationDocNumber;
                            operation.OperDate = date;
                            operation.DocDate = date;
                            operation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_MOL_STORAGE_IN);

                            //Создаем содержимое для МОЛ
                            MatPersonCardContent mpc = new MatPersonCardContent();
                            mpc.MatPersonCardHead = fromMatperson;
                            mpc.Quantity = -1 * item.WorkQuantity;
                            mpc.Storage = storage;
                            mpc.OperDate = date;
                            mpc.Wear = item.Wear;
                            mpc.Operation = operation;
                            mpc.OperType = operation.OperType;

                            storageRepository.SaveOrUpdate(storage);
                            operationRepository.SaveOrUpdate(operation);
                            matPersonCardContentRepository.SaveOrUpdate(mpc);
                        }
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", "При сохранении данных по номеклатуре `" + item.Nomenclature + "` произошла ошибка.\n"+e.Message);
                    }
                }

                if (ModelState.IsValid)
                {
                    operationRepository.DbContext.CommitTransaction();
                }
                else
                {
                    operationRepository.DbContext.RollbackTransaction();
                }

            }
            return View(new GridModel(new List<MatPersonOnHandsSimple>()));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_MOL_EDIT))]
        //Выдача МОЛ co cклада
        public ActionResult _outStorageSave(int toMatPersonId, string docDate, IEnumerable<MatPersonOnHandsSimple> updated)
        {
            DateTime date = DateTime.ParseExact(docDate, DataGlobals.DATE_FORMAT_FULL_YEAR, culture);
            date = date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);
            MatPersonCardHead fromMatperson = matPersonCardHeadRepository.Get(toMatPersonId);
            Dictionary<string, object> orderParams = new Dictionary<string, object>();
            orderParams.Add("Quantity", DESC);
            if ((fromMatperson != null) && (updated != null))
            {
                operationRepository.DbContext.BeginTransaction();
                string operationDocNumber = operationRepository.GetNextDocNumber(DataGlobals.OPERATION_WORKER_IN, fromMatperson.Organization.Id.ToString());
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                foreach (var item in updated)
                {
                    if (item.WorkQuantity <= 0)
                    {
                        continue;
                    }
                    try
                    {
                        //Ищем номенклатуру на складе
                        Storage storage = storageRepository.Get(item.NomenclatureId);
                        if (storage == null)
                        {
                            ModelState.AddModelError("", "Номенклатура `" + item.Nomenclature + "` на складе не найдена.");
                        }
                        else
                        {
                            if (item.WorkQuantity > storage.Quantity)
                            {
                                ModelState.AddModelError("", "На складе не хватает номенклатуры `" + item.Nomenclature + "`.");
                                continue;
                            }

                            storage.Quantity = storage.Quantity - item.WorkQuantity;

                            //Создаем операцию перемещения
                            Operation operation = new Operation();
                            operation.Organization = fromMatperson.Organization;
                            operation.Quantity = item.WorkQuantity;
                            operation.StorageName = fromMatperson.StorageName;
                            operation.Wear = item.Wear;
                            operation.Storage = storage;
                            operation.DocNumber = operationDocNumber;
                            operation.OperDate = date;
                            operation.DocDate = date;
                            operation.OperType = operTypeRepository.Get(DataGlobals.OPERATION_MOL_STORAGE_OUT);

                            //Создаем содержимое для МОЛ
                            MatPersonCardContent mpc = new MatPersonCardContent();
                            mpc.MatPersonCardHead = fromMatperson;
                            mpc.Quantity = item.WorkQuantity;
                            mpc.Storage = storage;
                            mpc.OperDate = date;
                            mpc.Wear = storage.Wear;
                            mpc.Operation = operation;
                            mpc.OperType = operation.OperType;

                            storageRepository.SaveOrUpdate(storage);
                            operationRepository.SaveOrUpdate(operation);
                            matPersonCardContentRepository.SaveOrUpdate(mpc);
                        }
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", "При сохранении данных по номеклатуре `" + item.Nomenclature + "` произошла ошибка.\n" + e.Message);
                    }
                }

                if (ModelState.IsValid)
                {
                    operationRepository.DbContext.CommitTransaction();
                }
                else
                {
                    operationRepository.DbContext.RollbackTransaction();
                }
            }
            return View(new GridModel(new List<MatPersonOnHandsSimple>()));
        }

        [HttpPost]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_MOL_EDIT + ", " + DataGlobals.ROLE_MOL_VIEW))]
        public ActionResult _GetNomenclaturesOnMatPerson(string normaContentId, string storageNameId, string text)
        {
            Dictionary<string, object> query = new Dictionary<string, object>();
            Dictionary<string, object> order = new Dictionary<string, object>();

            List<Storage> storages = new List<Storage>();

            int code = -1;
            if (text.Length > 2 && int.TryParse(text.Substring(2), out code))
                query.Add("Nomenclature.ExternalCode", text);
            else
            {
                if (text.Length <= 2) text = "";
                query.Add("Nomenclature.Name", text);
            }
            Organization currentOrg = organizationRepository.Get(int.Parse(getCurrentEnterpriseId()));
            query.Add("StorageName.Organization", currentOrg);
            query.Add("[>]Quantity", 0);
            query.Add("[!=]Wear", "50");
            order.Add("Wear", DESC);
            order.Add("Nomenclature.Sex.Id", ASC);
            order.Add("NomBodyPartSize.SizeNumber", ASC);

            if (normaContentId != null)
            {
                query.Add("StorageName.Id", int.Parse(storageNameId));
                NormaContent normaContent = normaContentRepository.Get(int.Parse(normaContentId));

                // сортировка по размеру, но если не будет размера у номенклатуры, то непопадет в выборку
                //if (normaContent.NomGroup.NomBodyPart != null && DataGlobals.SIZ_SIZE_ID != normaContent.NomGroup.NomBodyPart.Id)
                //{
                //    order.Add("Nomenclature.Sex.Id", ASC);
                //    order.Add("NomBodyPartSize.SizeNumber", ASC);
                //}

                StringBuilder nomGroupBase = new StringBuilder();
                StringBuilder nomGroups = new StringBuilder();
                foreach (var normaNomGroup in normaContent.NormaNomGroups)
                {
                    if (normaNomGroup.IsBase == true)
                    {
                        nomGroupBase.Append("'" + normaNomGroup.NomGroup.Id + "',");
                        break;
                    }
                }
                nomGroupBase.Remove(nomGroupBase.Length - 1, 1);

                foreach (var normaNomGroup in normaContent.NormaNomGroups)
                {
                    if (normaNomGroup.IsBase == true)
                        continue;
                    nomGroups.Append("'" + normaNomGroup.NomGroup.Id + "',");
                }

                query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());
                storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                if (nomGroups.Length > 0)
                {
                    query.Remove("[in]Nomenclature.NomGroup.Id");
                    nomGroups.Remove(nomGroups.Length - 1, 1);
                    query.Add("[in]Nomenclature.NomGroup.Id", nomGroups.ToString());
                    storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                }

                if (storages.Count == 0)
                {
                    order.Remove("Nomenclature.Sex.Id");
                    order.Remove("NomBodyPartSize.SizeNumber");

                    query.Remove("[in]Nomenclature.NomGroup.Id");
                    query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());
                    storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                    if (nomGroups.Length > 0)
                    {
                        query.Remove("[in]Nomenclature.NomGroup.Id");
                        query.Add("[in]Nomenclature.NomGroup.Id", nomGroups.ToString());
                        storages.AddRange(storageRepository.GetByLikeCriteria(query, order));
                    }
                }
            }
            else
                storages.AddRange(storageRepository.GetByLikeCriteria(query, order));

            return new JsonResult
            {
                Data = new SelectList(storages, "Id", "StorageInfo")
            };
        }

        private WorkerCardContent getWorkerCardPresent(IList<NormaNomGroup> normaNomGroups, IList<WorkerCardContent> workerCards)
        {
            int workerNomGroupId;
            WorkerCardContent outWorkerCard = new WorkerCardContent();
            NomGroup nomGroup;
            foreach (NormaNomGroup item in normaNomGroups)
            {
                nomGroup = item.NomGroup;
                foreach (WorkerCardContent curWorkerCard in workerCards)
                {
                    if (curWorkerCard.NormaContent != null)
                        workerNomGroupId = curWorkerCard.NormaContent.NomGroup.Id;

                    else
                        workerNomGroupId = curWorkerCard.Storage.Nomenclature.NomGroup.Id;

                    if (curWorkerCard.Quantity > 0 && nomGroup.Id == workerNomGroupId)
                    {
                        if (outWorkerCard.NormaContent == null)
                        {
                            outWorkerCard.Operation = new Operation();
                            outWorkerCard.Operation.OperDate = curWorkerCard.Operation.OperDate;
                            outWorkerCard.StartDate = curWorkerCard.StartDate;
                            outWorkerCard.Operation.DocNumber = curWorkerCard.Operation.DocNumber;
                            outWorkerCard.Storage = curWorkerCard.Storage;
                            outWorkerCard.NormaContent = curWorkerCard.NormaContent;
                            outWorkerCard.IsCorporate = curWorkerCard.IsCorporate;
                            outWorkerCard.OperationId = curWorkerCard.Operation.Id;
                            outWorkerCard.WorkerCardContentId = curWorkerCard.Id;
                            outWorkerCard.Operation = curWorkerCard.Operation;
                        }
                        //                      else
                        outWorkerCard.Quantity += curWorkerCard.Quantity;
                    }
                }
            }
            return outWorkerCard;
        }

        // рекурсивный метод поиска номера документа по которому производилась выдача, дабы после всех переводов найти все-таки этот номер
        private string getDocNumber(Operation oper)
        {
            string docNumber = "";
            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("OperReturn", oper);
            IList<WorkerCardContent> wccs = workerCardContentRepository.GetByLikeCriteria(query);
            if (wccs.Count > 0)
            {
                if (wccs[0].Operation.OperType.Id == DataGlobals.OPERATION_WORKER_IN)
                {
                    docNumber = wccs[0].Operation.DocNumber;
                    return docNumber;
                }
                else
                {
                    if (wccs[0].Operation.TransferOperation != null)
                        docNumber = getDocNumber(wccs[0].Operation.TransferOperation);
                }
            }
            return docNumber;
        }

        //Попытка найти подходящую номенклатуру у МОЛ для работника
        private MatPersonOnHands getStorageForWorkerNorma(NormaContent normaContent, Worker worker, StorageName storageName)
        {
            //return new MatPersonOnHands();
            List<MatPersonOnHands> storages = new List<MatPersonOnHands>();
            NomBodyPart normaBodyPart = normaContent.NomGroup.NomBodyPart;
            NomBodyPartSize workerBodyPartSize = null;

            // ищем размер сотрудника для данной нормы
            foreach (var item in worker.NomBodyPartSizes)
            {
                if (item.NomBodyPart.Id == normaBodyPart.Id)
                {
                    workerBodyPartSize = item;
                    break;
                }
            }

            Dictionary<string, object> query = new Dictionary<string, object>();
            Dictionary<string, object> order = new Dictionary<string, object>();

            // попытка найти полное соответствие по всем параметрам
            // в том числе и в группах замены
            query.Add("StorageName.Organization", organizationRepository.Get(int.Parse(getCurrentEnterpriseId())));
            query.Add("StorageName", storageName);
            query.Add("[>]Quantity", 0);
            query.Add("Wear", "100");
            if (worker.Sex != null)
            {
                query.Add("Nomenclature.Sex", worker.Sex);
            }
            if (workerBodyPartSize != null && DataGlobals.SIZ_SIZE_ID != normaBodyPart.Id)
            {
                Dictionary<string, object> q = new Dictionary<string, object>();
                q.Add("NomBodyPart", normaBodyPart);
                q.Add("SizeNumber", workerBodyPartSize.SizeNumber);
                IList<NomBodyPartSize> growth = nomBodyPartSizeRepository.GetByCriteria(q);
                if (growth.Count > 0)
                {
                    query.Add("Nomenclature.NomBodyPartSize", growth[0]);
                }

                order.Add("Nomenclature.NomBodyPartSize.SizeNumber", ASC);
            }
            if (DataGlobals.CLOTH_SIZE_ID == normaBodyPart.Id)
            {
                Dictionary<string, object> q = new Dictionary<string, object>();
                q.Add("NomBodyPart.Id",1);
                q.Add("SizeNumber",worker.Growth);
                IList<NomBodyPartSize> growth = nomBodyPartSizeRepository.GetByCriteria(q);
                if (growth.Count > 0)
                {
                    query.Add("Nomenclature.Growth", growth[0]);
                }
                order.Add("Nomenclature.Growth.SizeNumber", ASC);
            }
            order.Add("Wear", DESC);

            // формирование основной группы и списка групп замены
            StringBuilder nomGroupBase = new StringBuilder();
            StringBuilder nomGroups = new StringBuilder();
            foreach (var normaNomGroup in normaContent.NormaNomGroups)
            {
                if (normaNomGroup.IsBase == true)
                {
                    nomGroupBase.Append("'" + normaNomGroup.NomGroup.Id + "',");
                    break;
                }
            }
            nomGroupBase.Remove(nomGroupBase.Length - 1, 1);

            foreach (var normaNomGroup in normaContent.NormaNomGroups)
            {
                if (normaNomGroup.IsBase == true)
                    continue;
                nomGroups.Append("'" + normaNomGroup.NomGroup.Id + "',");
            }

            // разделил запросы по основной группе и группам замены,
            // чтобы сначала списка шли позиции по основной группе
            query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());
          //  storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query, order));
            storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query));


            if (nomGroups.Length > 0)
            {
                nomGroups.Remove(nomGroups.Length - 1, 1);
                query.Remove("[in]Nomenclature.NomGroup.Id");
                query.Add("[in]Nomenclature.NomGroup.Id", nomGroups.ToString());
                //storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query, order));
                storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query));
            }

            // если не нашли, то попытка поиска без роста и пола
            if (storages == null || storages.Count == 0)
            {
                query.Remove("Nomenclature.Growth");
                query.Remove("Nomenclature.Sex");
                query.Remove("[in]Nomenclature.NomGroup.Id");
                query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());
               // storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query, order));
                storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query));

                if (nomGroups.Length > 0)
                {
                    query.Remove("[in]Nomenclature.NomGroup.Id");
                    query.Add("[in]Nomenclature.NomGroup.Id", nomGroups.ToString());
                    storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query));
                    //storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query, order));
                }
            }

            // если не нашли, то попытка поиска без размера
            if (storages == null || storages.Count == 0)
            {
                query.Remove("Nomenclature.NomBodyPartSize");
                query.Remove("[in]Nomenclature.NomGroup.Id");
                query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());
                //storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query, order));
                storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query));

                if (nomGroups.Length > 0)
                {
                    query.Remove("[in]Nomenclature.NomGroup.Id");
                    query.Add("[in]Nomenclature.NomGroup.Id", nomGroups.ToString());
                    //storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query, order));
                    storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query));
                }
            }

            // если у позиций нет размеров, то из сортировки их нужно убрать,
            // чтобы запрос вернул хоть что-то
            if (storages == null || storages.Count == 0)
            {
                order.Remove("Nomenclature.NomBodyPartSize.SizeNumber");
                order.Remove("Nomenclature.Growth.SizeNumber");
                query.Remove("[in]Nomenclature.NomGroup.Id");
                query.Add("[in]Nomenclature.NomGroup.Id", nomGroupBase.ToString());
                //storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query, order));
                storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query));

                if (nomGroups.Length > 0)
                {
                    query.Remove("[in]Nomenclature.NomGroup.Id");
                    query.Add("[in]Nomenclature.NomGroup.Id", nomGroups.ToString());
                    //storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query, order));
                    storages.AddRange(matPersonOnHandsRepository.GetByLikeCriteria(query));
                }
            }
            // если совсем ничего не найдено, возвращаем пустую позицию
            if (storages == null || storages.Count == 0)
            {
                storages.Add(new MatPersonOnHands());
            }
            // попытка найти позицию большего размера
            if (storages.Count > 1 && workerBodyPartSize != null && DataGlobals.SIZ_SIZE_ID != normaBodyPart.Id)
            {
                String nextSizeNumber = null;
                for (int i = 1; i < 3; i++)
                {
                    nextSizeNumber = (double.Parse(workerBodyPartSize.SizeNumber) + i).ToString();
                    foreach (var item in storages)
                    {
                        if (item.Nomenclature.NomBodyPartSize != null && nextSizeNumber.Equals(item.Nomenclature.NomBodyPartSize.SizeNumber))
                            return item;
                    }
                }
            }
            return storages[0];
           
        }


        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_MOL_EDIT + ", " + DataGlobals.ROLE_MOL_VIEW))]
        public ActionResult Select_Worker(int? workerWorkplaceId)
        {
            IList<WorkerWorkplace> workerWorkplaces = new List<WorkerWorkplace>();
            //if (workerWorkplaceId == null && Session["workerWorkplaceId"] != null)
            //    workerWorkplaceId = (int)Session["workerWorkplaceId"];
            if (workerWorkplaceId != null)
            {
                WorkerWorkplace workerWorkplace = workerWorkplaceRepository.Get(workerWorkplaceId.Value);
                if (workerWorkplace != null) workerWorkplaces.Add(rebuildWorkerWorkplace(workerWorkplace));
                    else workerWorkplace = new WorkerWorkplace();
            }
            return View(new GridModel(workerWorkplaces));
        }

        // Для выдачи сотруднику от МОЛ
        [GridAction]
        [Transaction]
        [Authorize(Roles = (DataGlobals.ROLE_ADMIN + ", " + DataGlobals.ROLE_VIEW_ALL + ", " + DataGlobals.ROLE_MOL_EDIT + ", " + DataGlobals.ROLE_MOL_VIEW))]
        public ActionResult _SelectInPersonGrid(int? workerWorkplaceId, int fromMatPersonId)
        {
            IList<WorkerNorma> workerNormas = new List<WorkerNorma>();

            WorkerNorma workerNorma = null;
            MatPersonCardHead fromMatperson = matPersonCardHeadRepository.Get(fromMatPersonId);
            WorkerWorkplace workerWorkplace = workerWorkplaceRepository.Get(workerWorkplaceId.Value);
            if ((workerWorkplace != null) && (fromMatperson != null))
            {
                StorageName storageName = fromMatperson.StorageName;
                string storageNumber = storageName.StorageNumber;

                Dictionary<string, object> query = new Dictionary<string, object>();
                Dictionary<string, object> order = new Dictionary<string, object>();
                query.Add("Organization", workerWorkplace.Organization);
                query.Add("Norma.Organization", storageName.Organization);
                query.Add("Norma.IsActive", true);
                IList<NormaOrganization> normaOrganizations = normaOrganizationRepository.GetByCriteria(query);
                if (normaOrganizations != null && normaOrganizations.Count > 0)
                {
                    NormaOrganization normaOrganization = normaOrganizations[0];

                    // пересортировка - СИЗ в конец списка
                    IList<NormaContent> normaContents = reorderNormaContents(normaOrganization.Norma.NormaContents, true);

                    query.Clear();
                    query.Add("WorkerWorkplace", workerWorkplace);
                    WorkerCardHead workerCardHead = workerCardRepository.FindOne(query);
                    WorkerCardContent workerCardContent = null;

                    foreach (NormaContent curNormaContent in normaContents)
                    {
                        if (workerCardHead != null)
                            // ищем на руках номенклатуру по текущей группе номенклатур
                            workerCardContent = getWorkerCardPresent(curNormaContent.NormaNomGroups, workerCardHead.getActiveWorkerCardContent());
                        // на неактивном рабочем месте показывает только то, что есть на руках по норме
                        if (workerWorkplace.IsActive == false && workerCardContent == null)
                            continue;
                        //workerNormas.Add(new WorkerNorma(normaContent, workerCardContent, curNormaContent.Quantity));
                        //workerNorma = new WorkerNorma(nomGroup, workerCardContent, curNormaContent.Quantity);

                        workerNorma = new WorkerNorma();
                        //if (workerCardContent != null)
                        //    workerNorma.Storage = rebuildStorage(workerCardContent.Storage);
                        //else
                        //    workerNorma.Storage = rebuildStorage(storage);
                        workerNorma.IsCorporate = false;
                        workerNorma.NormaContentId = curNormaContent.Id;
                        workerNorma.NormaContentName = curNormaContent.NomGroup.Name;
                        workerNorma.NormaQuantity = curNormaContent.Quantity;
                        workerNorma.NormaUsePeriod = curNormaContent.UsePeriod;
                        //workerNorma.PutQuantity = curNormaContent.Quantity;
                        if (workerCardContent != null)
                        {
                            if (workerCardContent.Quantity > 0)
                            {
                                if (workerCardContent.Operation.OperType.Id == DataGlobals.OPERATION_STORAGE_TRANSFER_IN && workerCardContent.Operation.TransferOperation != null)
                                {
                                    workerNorma.DocNumber = getDocNumber(workerCardContent.Operation.TransferOperation);
                                }
                                else
                                    workerNorma.DocNumber = workerCardContent.Operation.DocNumber;
                                workerNorma.PresentQuantity = workerCardContent.Quantity;
                                workerNorma.ReceptionDate = workerCardContent.StartDate;
                                //                                workerNorma.DocNumber = workerCardContent.Operation.DocNumber;
                                workerNorma.StorageId = workerCardContent.Storage.Nomenclature.Id;
                                workerNorma.StorageNumber = workerCardContent.Storage.StorageName.StorageNumber;
                                workerNorma.StorageInfo = workerCardContent.Storage.StorageInfo;
                                workerNorma.IsCorporate = workerCardContent.IsCorporate;
                                workerNorma.OperationId = workerCardContent.OperationId;
                                workerNorma.OperTypeId = workerCardContent.Operation.OperType.Id;
                                workerNorma.Wear = workerCardContent.Storage.Wear;
                                workerNorma.WorkerCardContentId = workerCardContent.WorkerCardContentId;
                                //Удаляем номенклатуру из списка поиска.
                                //Иначе могут двоиться записи (если основная группа и группа замены
                                //ссылаются друг на друга)
                                workerCardHead.removeWorkerCardContent(workerCardContent);
                            }
                            else
                            {
                                MatPersonOnHands storage = getStorageForWorkerNorma(curNormaContent, workerWorkplace.Worker, fromMatperson.StorageName);
                                if (storage.Nomenclature != null)
                                {
                                    workerNorma.StorageId = storage.Nomenclature.Id;
                                    workerNorma.StorageInfo = storage.StorageInfo;
                                    workerNorma.Wear = storage.Wear;
                                }

                                if (storage.Id != 0)
                                    workerNorma.StorageNumber = storage.StorageName.StorageNumber;
                                else
                                    workerNorma.StorageNumber = storageNumber;
                            }
                        }
                        else
                        {
                            MatPersonOnHands storage = getStorageForWorkerNorma(curNormaContent, workerWorkplace.Worker, fromMatperson.StorageName);
                            if (storage.Nomenclature != null)
                            {
                                workerNorma.StorageId = storage.Nomenclature.Id;
                                workerNorma.StorageInfo = storage.StorageInfo;
                                workerNorma.Wear = storage.Wear;
                            }
                            workerNorma.Wear = storage.Wear;

                            if (storage.Id != 0)
                                workerNorma.StorageNumber = storage.StorageName.StorageNumber;
                            else
                                workerNorma.StorageNumber = storageNumber;
                        }
                        if (workerCardContent != null)
                            if (workerCardContent.Quantity > 0)
                            {
                                workerNorma.PresentQuantity = workerCardContent.Quantity;
                                DateTime dt = workerCardContent.Operation.OperDate.AddMonths(curNormaContent.UsePeriod);
                                //double qq = double.Parse((workerNorma.PresentQuantity / workerNorma.PutQuantity).ToString());
                                //                                if (workerNorma.PutQuantity > 0 && workerCardContent.Operation.OperDate.AddMonths(curNormaContent.UsePeriod * (workerNorma.PresentQuantity / workerNorma.PutQuantity)) > DateTime.Now)
                                //if (workerNorma.PutQuantity > 0 && workerNorma.PresentQuantity > 0)
                                //    workerNorma.PutQuantity -= workerCardContent.Quantity;
                                //else
                                //    workerNorma.PutQuantity = workerCardContent.Quantity;
                                //                                workerNorma.ReceptionDate = workerCardContent.Operation.OperDate;
                                workerNorma.ReceptionDate = workerCardContent.StartDate;
                            }
                        // Удаляем из списка позиции номенклатур, которые уже все выданы по норме и выдавать больше не требуется
//                        if (workerNorma.PresentQuantity < workerNorma.NormaQuantity){
                            workerNormas.Add(workerNorma);
//                        }
                    }

                }

            }
            return View(new GridModel(workerNormas));

        }

    }
}
