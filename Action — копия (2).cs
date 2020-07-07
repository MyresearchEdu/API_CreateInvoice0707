//---------------------------------------------
//Script Title        :
//Script Description  :
//
//
//Recorder Version    : 
//---------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.Framework.UI;
using Microsoft.Dynamics.Nav.Client;
using Microsoft.Dynamics.Nav.Client.WinClient;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Client.Visualizations;
using Microsoft.Dynamics.Nav.Types.Data;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.Client.Actions;
using System.Data.SqlClient;
using System.Timers;

using System.Globalization;// culture


using System.Diagnostics; // stopwatch

//using System.IO;
//using System.Collections.Generic; //for dictionary
using System.Runtime.InteropServices; //for P/Invoke DLLImport
using System.Data;

using System.Windows.Automation;




namespace Script
{

    public static class AutomationExtensions
    {
        public static void EnsureElementIsScrolledIntoView(this AutomationElement element)
        {
            if (!element.Current.IsOffscreen)
            {
                return;
            }

            if (!(bool)element.GetCurrentPropertyValue(AutomationElement.IsScrollItemPatternAvailableProperty))
            {
                return;
            }

            var scrollItemPattern = element.GetScrollItemPattern();
            scrollItemPattern.ScrollIntoView();
        }

        public static AutomationElement FindDescendentByConditionPath(this AutomationElement element, IEnumerable<Condition> conditionPath)
        {
            if (!conditionPath.Any())
            {
                return element;
            }

            var result = conditionPath.Aggregate(
                element,
                (parentElement, nextCondition) => parentElement == null
                                                      ? null
                                                      : parentElement.FindChildByCondition(nextCondition));

            return result;
        }

        public static AutomationElement FindDescendentByIdPath(this AutomationElement element, IEnumerable<string> idPath)
        {
            var conditionPath = CreateConditionPathForPropertyValues(AutomationElement.AutomationIdProperty, idPath.Cast<object>());

            return FindDescendentByConditionPath(element, conditionPath);
        }

        public static AutomationElement FindDescendentByNamePath(this AutomationElement element, IEnumerable<string> namePath)
        {
            var conditionPath = CreateConditionPathForPropertyValues(AutomationElement.NameProperty, namePath.Cast<object>());

            return FindDescendentByConditionPath(element, conditionPath);
        }

        public static IEnumerable<Condition> CreateConditionPathForPropertyValues(AutomationProperty property, IEnumerable<object> values)
        {
            var conditions = values.Select(value => new PropertyCondition(property, value));

            return conditions.Cast<Condition>();
        }
        /// <summary>
        /// Finds the first child of the element that has a descendant matching the condition path.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="conditionPath">The condition path.</param>
        /// <returns></returns>
        public static AutomationElement FindFirstChildHavingDescendantWhere(this AutomationElement element, IEnumerable<Condition> conditionPath)
        {
            return
                element.FindFirstChildHavingDescendantWhere(
                    child => child.FindDescendentByConditionPath(conditionPath) != null);
        }

        /// <summary>
        /// Finds the first child of the element that has a descendant matching the condition path.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="conditionPath">The condition path.</param>
        /// <returns></returns>
        public static AutomationElement FindFirstChildHavingDescendantWhere(this AutomationElement element, Func<AutomationElement, bool> condition)
        {
            var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);

            foreach (AutomationElement child in children)
            {
                if (condition(child))
                {
                    return child;
                }
            }

            return null;
        }

        public static AutomationElement FindChildById(this AutomationElement element, string automationId)
        {
            var result = element.FindChildByCondition(
                new PropertyCondition(AutomationElement.AutomationIdProperty, automationId));

            return result;
        }

        public static AutomationElement FindChildByName(this AutomationElement element, string name)
        {
            var result = element.FindChildByCondition(
                new PropertyCondition(AutomationElement.NameProperty, name));

            return result;
        }

        public static AutomationElement FindChildByClass(this AutomationElement element, string className)
        {
            var result = element.FindChildByCondition(
                new PropertyCondition(AutomationElement.ClassNameProperty, className));

            return result;
        }

        public static AutomationElement FindChildByProcessId(this AutomationElement element, int processId)
        {
            var result = element.FindChildByCondition(
                new PropertyCondition(AutomationElement.ProcessIdProperty, processId));

            return result;
        }

        public static AutomationElement FindChildByControlType(this AutomationElement element, System.Windows.Automation.ControlType controlType)
        {
            var result = element.FindChildByCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, controlType));

            return result;
        }

        public static AutomationElement FindChildByCondition(this AutomationElement element, Condition condition)
        {
            var result = element.FindFirst(
                TreeScope.Children,
                condition);

            return result;
        }

        /// <summary>
        /// Finds the child text block of an element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static AutomationElement FindChildTextBlock(this AutomationElement element)
        {
            var child = TreeWalker.RawViewWalker.GetFirstChild(element);

            if (child != null && child.Current.ControlType == System.Windows.Automation.ControlType.Text)
            {
                return child;
            }

            return null;
        }
    }

    public static class PatternExtensions
    {
        public static string GetValue(this AutomationElement element)
        {
            var pattern = element.GetPattern<ValuePattern>(ValuePattern.Pattern);

            return pattern.Current.Value;
        }

        public static void SetValue(this AutomationElement element, string value)
        {
            var pattern = element.GetPattern<ValuePattern>(ValuePattern.Pattern);

            pattern.SetValue(value);
        }

        public static ScrollItemPattern GetScrollItemPattern(this AutomationElement element)
        {
            return element.GetPattern<ScrollItemPattern>(ScrollItemPattern.Pattern);
        }

        public static InvokePattern GetInvokePattern(this AutomationElement element)
        {
            return element.GetPattern<InvokePattern>(InvokePattern.Pattern);
        }

        public static SelectionItemPattern GetSelectionItemPattern(this AutomationElement element)
        {
            return element.GetPattern<SelectionItemPattern>(SelectionItemPattern.Pattern);
        }

        public static SelectionPattern GetSelectionPattern(this AutomationElement element)
        {
            return element.GetPattern<SelectionPattern>(SelectionPattern.Pattern);
        }

        public static TogglePattern GetTogglePattern(this AutomationElement element)
        {
            return element.GetPattern<TogglePattern>(TogglePattern.Pattern);
        }

        public static WindowPattern GetWindowPattern(this AutomationElement element)
        {
            return element.GetPattern<WindowPattern>(WindowPattern.Pattern);
        }

        public static T GetPattern<T>(this AutomationElement element, AutomationPattern pattern) where T : class
        {
            var patternObject = element.GetCurrentPattern(pattern);

            return patternObject as T;
        }


    }














    public static class asdasd
    {

        public static void CloseModalWindows()
        {
            // get the main window
            AutomationElement root = AutomationElement.FromHandle(Process.GetCurrentProcess().MainWindowHandle);
            //AutomationElement root = AutomationElement.FromHandle(Process.GetCurrentProcess().Handle);
            if (root == null) return;


            // it should implement the Window pattern
            object pattern;
            if (!root.TryGetCurrentPattern(WindowPattern.Pattern, out pattern)) return;

            WindowPattern window = (WindowPattern)pattern;
            if (window.Current.WindowInteractionState != WindowInteractionState.ReadyForUserInteraction)
            {
                // get sub windows
                foreach (AutomationElement element in root.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Window)))
                {
                    // hmmm... is it really a window?
                    if (element.TryGetCurrentPattern(WindowPattern.Pattern, out pattern))
                    {
                        // if it's ready, try to close it
                        WindowPattern childWindow = (WindowPattern)pattern;
                        if (childWindow.Current.WindowInteractionState == WindowInteractionState.ReadyForUserInteraction)
                        {
                            childWindow.Close();
                        }
                    }
                }
            }
        }
    }



        public static class Encodinggg
    {
        public static string EncodeBase64(this System.Text.Encoding encoding, string text)
        {

            if (text == null)
            {
                return null;
            }

            byte[] textAsBytes = encoding.GetBytes(text);
            return System.Convert.ToBase64String(textAsBytes);
        }

        public static string DecodeBase64(this System.Text.Encoding encoding, string encodedText)
        {
            if (encodedText == null)
            {
                return null;
            }

            byte[] textAsBytes = System.Convert.FromBase64String(encodedText);
            return encoding.GetString(textAsBytes);
        }
    }


    public partial class VuserClass
    {





        static byte[] poav_pattern = new byte[] { 208, 159, 208, 158, 208, 144, 208, 146 }; // ПОАВ

        static byte[] poav_pattern2 = new byte[] { 38, 0, 0, 0, 0, 139, 2, 0, 0, 0, 2, 123, 255, 31, 4, 30, 4, 16, 4, 18, 4 }; // &{ //-20-148
        static byte[] poav_pattern3 = new byte[] { 39, 0, 0, 0, 0, 139, 2, 0, 0, 0, 2, 123, 255, 31, 4, 30, 4, 16, 4, 18, 4 }; // &{ //-20-148

        static byte[] correlate_poav_number_wrapper_string(string referal, byte[] POAV_NUMBER)
        {
            byte[] sb_1 = Convert.FromBase64String(referal);

            string datastr_original70 = System.Text.Encoding.UTF8.GetString((byte[])sb_1);

            correlate_poav_number(sb_1, POAV_NUMBER);

            string datastr_original71 = System.Text.Encoding.UTF8.GetString((byte[])sb_1);

            return sb_1;

        }

        static NavDataSet correlate_data_filed_wrapper(NavDataSet nds_orig, byte[] POAV_NUMBER, string InvoiceContentStr)
        {

            //извлекаем из структуры NAV отражением и проверяем что внурри до перезаписи
            var nds_DS_orig = nds_orig.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var nds_DS_orig_val = nds_DS_orig.GetValue(nds_orig);
            byte[] nds_D_orig_byte = (byte[])nds_DS_orig_val.GetType().GetProperty("Data").GetValue(nds_DS_orig_val);

            string nds_orig_str = System.Text.Encoding.UTF8.GetString((byte[])nds_D_orig_byte);

            //************************************
            //       DATA FIELD CORRELATION
            //записываем в структуру NAV отражением
            byte[] InvoiceContentByte = Convert.FromBase64String(InvoiceContentStr);

            string invoice_orig_str = System.Text.Encoding.UTF8.GetString((byte[])InvoiceContentByte);


            for (int i = 0; i < InvoiceContentByte.Length; i++)
            {
                if (InvoiceContentByte.Skip(i).Take(poav_pattern.Length).SequenceEqual(poav_pattern))
                {
                    correlate_data_field(i + 8, InvoiceContentByte, POAV_NUMBER);
                }

                if (InvoiceContentByte.Skip(i).Take(poav_pattern2.Length).SequenceEqual(poav_pattern2))
                {
                    correlate_data_field_id2(i + 21, InvoiceContentByte, POAV_NUMBER);
                }

                if (InvoiceContentByte.Skip(i).Take(poav_pattern3.Length).SequenceEqual(poav_pattern3))
                {
                    correlate_data_field_id2(i + 21, InvoiceContentByte, POAV_NUMBER);
                }
            }

            NavDataSet nds_corr = new NavDataSet { DataSetName = "Purchase Header" };

            // КОРРЕЛЯЦИЯ ЗДЕСЬ
            var nds_DS_corr = nds_corr.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(nds_corr);
            nds_DS_corr.GetType().GetProperty("Data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(nds_DS_corr, InvoiceContentByte);
            nds_corr.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(nds_corr, nds_DS_corr);

            //извлекаем из структуры NAV отражением и проверяем что записалось
            var nds_DS_corr_2 = nds_corr.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var nds_DS_corr_val = nds_DS_corr_2.GetValue(nds_corr);
            byte[] nds_D_corr_byte = (byte[])nds_DS_corr_val.GetType().GetProperty("Data").GetValue(nds_DS_corr_val);

            string nds_corr_str = System.Text.Encoding.UTF8.GetString((byte[])nds_D_corr_byte);

            return nds_corr;
        }



        static string evaluate_data_set(NavDataSet MyDataSet)
        {
            var recdatasetref_2 = MyDataSet.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var recdataset_2 = recdatasetref_2.GetValue(MyDataSet);
            byte[] databyte_2 = (byte[])recdataset_2.GetType().GetProperty("Data").GetValue(recdataset_2);

            return System.Text.Encoding.UTF8.GetString((byte[])databyte_2);
        }

        static NavDataSet write_data_set(string NewDataSetState)
        {
            byte[] MyDataSet = Convert.FromBase64String(NewDataSetState);

            NavDataSet NavDataSet_0 = new NavDataSet { DataSetName = "Purchase Header" };

            var datasetdata_0 = NavDataSet_0.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(NavDataSet_0);
            datasetdata_0.GetType().GetProperty("Data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(datasetdata_0, MyDataSet);
            NavDataSet_0.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(NavDataSet_0, datasetdata_0);

            return NavDataSet_0;   //запись правильного поля Data, автоматически генерируется неправильно	
        }


        static NavDataSet correlate_data_field_COMPLEATE(NavDataSet iam_nds_3, byte[] POAV_NUMBER)
        {

            var recdatasetref7 = iam_nds_3.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var recdataset7 = recdatasetref7.GetValue(iam_nds_3);
            byte[] databyte7 = (byte[])recdataset7.GetType().GetProperty("Data").GetValue(recdataset7);

            for (int i = 0; i < databyte7.Length; i++)
            {

                if (databyte7.Skip(i).Take(poav_pattern.Length).SequenceEqual(poav_pattern))
                {
                    correlate_data_field(i + 8, databyte7, POAV_NUMBER);
                }

                if (databyte7.Skip(i).Take(poav_pattern2.Length).SequenceEqual(poav_pattern2))
                {
                    correlate_data_field_id2(i + 21, databyte7, POAV_NUMBER);
                }

                if (databyte7.Skip(i).Take(poav_pattern3.Length).SequenceEqual(poav_pattern3))
                {
                    correlate_data_field_id2(i + 21, databyte7, POAV_NUMBER);
                }
            }

            recdataset7.GetType().GetProperty("Data").SetValue(recdataset7, databyte7);

            recdatasetref7.SetValue(iam_nds_3, recdataset7);

            return iam_nds_3;
        }


        private static void asdasd(Object source, System.Timers.ElapsedEventArgs e) {
            AutomationElement mainWindow = AutomationElement.RootElement;

            var winCollection = mainWindow.FindAll(TreeScope.Children, Condition.TrueCondition);

            foreach (AutomationElement element in winCollection)
            {
                Debug.WriteLine("________________Line_____________" + element.Current.Name.ToString());
                if (element.Current.Name == "Подключение к win-ocnjuhb1lag")
                {
                    var closeButton = element.FindDescendentByIdPath(new[] { "TitleBar", "Close" });
                    if (closeButton != null)
                    {
                        //closeButton.GetInvokePattern().Invoke();
                    }
                }
            }
        }

        public int Action()
        {

            

            byte[] POAV_NUMBER = { 0 };
            byte[] POAV_NUMBER_lower_bookmark = { 0 };


            ///asdasd.Automate();

            //asdasd.CloseModalWindows();

            Timer time1 = new Timer();
            time1.Elapsed += new ElapsedEventHandler(asdasd);
            time1.Interval = 5000; // 30 secs

            time1.AutoReset = true;    // Have the timer fire repeated events (true is the default)

            time1.Enabled = true; // Start the timer
            

















            //*************************************************
            //  ЛОГИН
            //*************************************************

            //==============================================
            //  OpenConnection, SetClientSettings, GetAddInRegistrations
            // Старт клиента            
            lr.start_transaction("1_Login");




            IProductNameProvider productNameProvider = new StaticProductNameProvider("ПерфомансЛаб"); 
            NavWinFormsClientSessionPreparation navWinFormsClientSessionPreparation = new NavWinFormsClientSessionPreparation(productNameProvider, false); 
            NavWinFormsClientSession navWinFormsClientSession = new NavWinFormsClientSession(navWinFormsClientSessionPreparation); 

            UserNamePasswordCredentials unpc = new UserNamePasswordCredentials();
            unpc.UserName = "TESTUSER1";
            unpc.Password = "\\\\\\eRMp2kLgwMLqiiVkMukGZw==";

            lr.end_transaction("1_Login", 1);

            lr.start_transaction("2_Login");


            navWinFormsClientSession.Init(); 

            String personalizationId = navWinFormsClientSession.UISession.SessionPersonalizationId; //a9("SessionPersonalizationId", sw_5, sw_6, (short)7);

            String personalizationId1 = "ec29feaf-58ae-4c7a-8c7f-792d5cfd9aa0";
            String personalizationId2 = "374f138d-58a0-4b24-b6f9-da451371b033";

            IService service = navWinFormsClientSession.BuilderSession.Service; //a9("Service", sw_6, sw_7, (short)7);
            IMetadataService metadataService = navWinFormsClientSession.BuilderSession.MetadataService; //a9("MetadataService", sw_7, sw_8, (short)7);

            //==============================================
            //  GetPage

            //==============================================
            //  GetPage

            //==============================================
            //  GetNavigationFrame
            // Открытие панелей клиента
            MasterNavigation masterNavigation1 = metadataService.GetNavigationFrame(
                applyPersonalization: true,
                getNavigationFrameFromServer: true
                );

            //==============================================
            //  GetNavigationFrame
            MasterNavigation masterNavigation2 = metadataService.GetNavigationFrame(
                applyPersonalization: true,
                getNavigationFrameFromServer: true
                );

            // Получение главного окна
            ActionDefinition control_Role_Center = masterNavigation2.NavigationPane.
                    Actions.Single(a => a.ALIdentifier == "Control_Home").
                    Actions.Single(a => a.Name == "Control_Role_Center") as ActionDefinition;

            //==============================================
            //  GetMetadataForPageAndAllItsDependencies
            MasterPage masterPage = metadataService.GetMasterPage(
                pageId: control_Role_Center.TargetID, // 9006
                personalizationId: personalizationId1,
                applyPersonalization: true
                );
            //MasterPage myNotes = metadataService.GetSystemPart(
            //    partType: SystemPartTypes.MyNotes
            //    );
            //MetaTable myNotesTable = metadataService.GetTableMetadata(
            //    tableId: myNotes.PageProperties.SourceObject.SourceTable // 2000000068
            //    );

            //==============================================
            //  GetWorkDate
            DateTime workDate = service.GetWorkDate();

            lr.end_transaction("2_Login", 1);





            //*************************************************
            //  ПОИСК ОКНА ОТЧЕТОВ
            //*************************************************
            //==============================================
            //  GetMetadataForPageAndAllItsDependencies

            MasterPage gmfpaaid_1 = metadataService.GetMasterPage(
                pageId: 35610,
                personalizationId: personalizationId2,
                applyPersonalization: true
            );

            //CodeUnitResponse aaaa = service.InvokeApplicationMethod();


            //==============================================
            //  lnvokeApplicationMethod
            NavVisualizationHelper.Initialize(service);


            //==============================================
            //  OpenForm
            string nofa_1_mf_cr = "JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADAAMAAyADYAAAAAAA==";

            NavOpenFormArguments nofa_1_mf = new NavOpenFormArguments()
            {
                State = new NavRecordState()
                {
                    CurrentRecord = Convert.FromBase64String(nofa_1_mf_cr),
                    FormId = 35610, // gmfpaaid_1.ID, 
                    PersonalizationId = personalizationId2,
                    ParentFormId = 0,
                    NavFormEditable = false,
                    RenamingMode = RenamingMode.SingleKeyServerSide,
                    TableView = new NavTableView()
                    {
                        Ascending = true,
                        TableId = 38,//gmfpaaid_1.PageProperties.SourceObject.SourceTable,
                        SearchFilter = new NavFilterGroup()
                        {
                            FilterGroupNo = -1,
                            Filters = new NavFilter[0]
                        }
                    },
                    ValidateFieldsInOnNewRecord = true,
                    ValidateRequired = true
                }
            };
            NavOpenFormArguments[] nofa_1_sf = null;

            service.OpenForm(
                    mainForm: ref nofa_1_mf,
                    subForms: ref nofa_1_sf
                );

            //***************************************************
            //FORM 1 STATES DUPLICATE COPY 
            NavRecordState mf_1_s_c_0 = nofa_1_mf.State;
            NavRecordState mf_1_s_c_1 = nofa_1_mf.State;
            NavRecordState mf_1_s_c_2 = nofa_1_mf.State;
            NavRecordState mf_1_s_c_3 = nofa_1_mf.State;
            NavRecordState mf_1_s_c_4 = nofa_1_mf.State;
            NavRecordState mf_1_s_c_5 = nofa_1_mf.State;
            NavRecordState mf_1_s_c_6 = nofa_1_mf.State;

            NavRecordState mf_1_s_c_7 = nofa_1_mf.State;

            NavRecordState mf_1_s_c_8 = nofa_1_mf.State;
            NavRecordState mf_1_s_c_9 = nofa_1_mf.State;

            NavDataSet mf_1_ds_c_7 = nofa_1_mf.DataSet;

            //***************************************************


            //==============================================
            //  GetPage
            //NavRecordState gp_mf_1 = nofa_1_mf.State;
            string gp_sb_1 = "JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADAAMAAyADYAAAAAAA==";


            service.GetPage(
                pageRequestDefinition: new PageRequestDefinition()
                {
                    ExcludeStartingRecord = false,
                    IncludeMoreDataInformation = true,
                    IncludeNonRowData = true,
                    IsSubFormUpdateRequest = false,
                    LookupFieldIds = null,
                    LookupFieldValues = null,
                    NormalFields = null,
                    PageSize = 50,
                    PageSizeInOppositeDirection = 50,
                    ReadDirection = ReadDirection.Next,
                    StartFromPage = StartingPage.Specific,
                    StartingBookmark = Convert.FromBase64String(gp_sb_1)
                },
                state: ref mf_1_s_c_0
                );

            //==============================================
            //  GetPage
            //NavRecordState gp_mf_1 = nofa_1_mf.State;

            mf_1_s_c_1.CurrentRecord = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADAAMQAxADMAAAAAAA==");
            mf_1_s_c_1.InsertLowerBoundBookmark = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADAAMQAxADIAAAAAAA==");
            mf_1_s_c_1.InsertUpperBoundBookmark = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADAAMQAxADMAAAAAAA==");

            service.GetPage(
                pageRequestDefinition: new PageRequestDefinition()
                {
                    ExcludeStartingRecord = true,
                    IncludeMoreDataInformation = true,
                    IncludeNonRowData = false,
                    IsSubFormUpdateRequest = false,
                    LookupFieldIds = null,
                    LookupFieldValues = null,
                    NormalFields = null,
                    PageSize = 50,
                    PageSizeInOppositeDirection = 0,
                    ReadDirection = ReadDirection.Previous,
                    StartFromPage = StartingPage.Specific,
                    StartingBookmark = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADAAMAA2ADAAAAAAAA==")
                },
                state: ref mf_1_s_c_1
                );


            //*************************************************
            //  СОЗДАНИЕ ОТЧЕТА
            //*************************************************

            //==============================================
            //  GetMetadataForPageAndAllItsDependencies

            MasterPage gmfpaaid_2 = metadataService.GetMasterPage(
                pageId: 12431,
                personalizationId: string.Empty,
                applyPersonalization: true
            );

            //==============================================
            //  OpenForm
            NavOpenFormArguments nofa_2_mf = new NavOpenFormArguments()
            {
                ControlId = null,

                State = new NavRecordState()
                {
                    CurrentRecord = null,
                    FormId = 12431, //gmfpaaid_2.ID
                    //PersonalizationId = null,
                    ParentFormId = 0,
                    NavFormEditable = true,
                    RenamingMode = RenamingMode.SingleKeyServerSide,
                    TableView = new NavTableView()
                    {
                        Ascending = true,
                        TableId = 38,
                        CurrentSortingFieldIds = new int[2] { 1, 3 },
                        SearchFilter = new NavFilterGroup()
                        {
                            FilterGroupNo = -1,
                            Filters = new NavFilter[0]
                        },
                        CurrentFilters = new NavFilterGroup[1] {
                            new NavFilterGroup() {
                                FilterGroupNo = 2,
                                Filters = new NavFilter[2]{
                                    new NavFilter(){
                                        FilterField = 1,
                                        FilterValue = "2",
                                        IsExactValue = true,
                                        OptionsAsCaptionsFilterValue = "Invoice",
                                        UserTypedFilterValue = null
                                    },
                                    new NavFilter(){
                                        FilterField = 12400,
                                        FilterValue = "1",
                                        IsExactValue = true,
                                        OptionsAsCaptionsFilterValue = "Yes",
                                        UserTypedFilterValue = null
                                    }
                                }
                            }
                        }
                    },
                    ValidateFieldsInOnNewRecord = true,
                    ValidateRequired = true
                }
            };
            NavOpenFormArguments[] nofa_2_sf = new List<NavOpenFormArguments>().Append(new NavOpenFormArguments()
            {
                ControlId = "1210001",

                State = new NavRecordState()
                {
                    CurrentRecord = null,
                    FormId = 12432,
                    //PersonalizationId = personalizationId2,
                    ParentFormId = 12431,
                    NavFormEditable = true,
                    RenamingMode = RenamingMode.NoKeys,
                    TableView = new NavTableView()
                    {
                        Ascending = true,
                        TableId = 39,
                        SearchFilter = new NavFilterGroup()
                        {
                            FilterGroupNo = -1,
                            Filters = new NavFilter[0]
                        }
                    },
                    ValidateFieldsInOnNewRecord = true,
                    ValidateRequired = true
                }
            }).ToArray();

            service.OpenForm(
                        mainForm: ref nofa_2_mf,
                        subForms: ref nofa_2_sf
                    );

            //***************************************************
            //FORM 2 STATES DUPLICATE COPY 
            NavRecordState mf_2_s_c_0 = nofa_2_mf.State;
            NavRecordState mf_2_s_c_1 = nofa_2_mf.State;
            NavRecordState mf_2_s_c_2 = nofa_2_mf.State;
            NavRecordState mf_2_s_c_3 = nofa_2_mf.State;
            NavRecordState mf_2_s_c_4 = nofa_2_mf.State;

            NavDataSet mf_2_ds_c_0 = nofa_2_mf.DataSet;
            NavDataSet mf_2_ds_c_1 = nofa_2_mf.DataSet;
            NavDataSet mf_2_ds_c_2 = nofa_2_mf.DataSet;
            NavDataSet mf_2_ds_c_3 = nofa_2_mf.DataSet;
            NavDataSet mf_2_ds_c_4 = nofa_2_mf.DataSet;

            NavRecordState sf_2_s_1_c_0 = nofa_2_sf[0].State;
            NavRecordState sf_2_s_1_c_1 = nofa_2_sf[0].State;
            NavRecordState sf_2_s_1_c_2 = nofa_2_sf[0].State;
            NavRecordState sf_2_s_1_c_3 = nofa_2_sf[0].State;
            NavRecordState sf_2_s_1_c_4 = nofa_2_sf[0].State;
            NavRecordState sf_2_s_1_c_5 = nofa_2_sf[0].State;

            NavDataSet sf_2_ds_1_c_0 = nofa_2_sf[0].DataSet;
            NavDataSet sf_2_ds_1_c_1 = nofa_2_sf[0].DataSet;
            NavDataSet sf_2_ds_1_c_2 = nofa_2_sf[0].DataSet;
            NavDataSet sf_2_ds_1_c_3 = nofa_2_sf[0].DataSet;
            NavDataSet sf_2_ds_1_c_4 = nofa_2_sf[0].DataSet;
            NavDataSet sf_2_ds_1_c_5 = nofa_2_sf[0].DataSet;

            NavDataSet sf_2_ds_1_c_6 = nofa_2_sf[0].DataSet;
            //***************************************************

            //}
            //==============================================
            //  lnvokeApplicationMethod
            //NavRecordState iam_s_1 = nofa_2_mf.State;
            //NavDataSet iam_nds_1 = nofa_2_mf.DataSet;
            int iam_s_fid_1 = nofa_2_mf.State.FormId;
            object[] iam_obj_1 = new object[1]; iam_obj_1[0] = true;

            //извлекаем из структуры NAV отражением и проверяем что внурри до перезаписи           
            string datastr_original_1 = evaluate_data_set(mf_2_ds_c_0);

            //************************************
            //       DATA FIELD CORRELATION
            //записываем в структуру NAV отражением   //запись правильного поля Data, автоматически генерируется неправильно
            mf_2_ds_c_0 = write_data_set("B2RhdGFTZXR/AAAAAQAAAA9QdXJjaGFzZSBIZWFkZXIJBAAAswAAAAEwCgAAAAAIYm9va21hcmsTAAAAAAExCAAAAAABMhEAAAAAATMRAAAAAAE0EQAAAAABNREAAAAAATYRAAAAAAE3EQAAAAABOBEAAAAAATkRAAAAAAIxMBEAAAAAAjExEQAAAAACMTIRAAAAAAIxMxEAAAAAAjE0EQAAAAACMTURAAAAAAIxNhEAAAAAAjE3EQAAAAACMTgRAAAAAAIxOQ8AAAAAAjIwDwAAAAACMjEPAAAAAAIyMhEAAAAAAjIzEQAAAAACMjQPAAAAAAIyNQ4AAAAAAjI2DwAAAAACMjcRAAAAAAIyOBEAAAAAAjI5EQAAAAACMzARAAAAAAIzMREAAAAAAjMyEQAAAAACMzMOAAAAAAIzNQIAAAAAAjM3EQAAAAACNDERAAAAAAI0MxEAAAAAAjQ1EQAAAAACNDYCAAAAAAI0NwgAAAAAAjUxEQAAAAACNTIIAAAAAAI1MxEAAAAAAjU1EQAAAAACNTYCAAAAAAI1NwIAAAAAAjU4AgAAAAACNTkCAAAAAAI2MA4AAAAAAjYxDgAAAAACNjIRAAAAAAI2MxEAAAAAAjY0EQAAAAACNjURAAAAAAI2NhEAAAAAAjY3EQAAAAACNjgRAAAAAAI2OREAAAAAAjcwEQAAAAACNzIRAAAAAAI3MxEAAAAAAjc0EQAAAAACNzYRAAAAAAI3NxEAAAAAAjc4EQAAAAACNzkRAAAAAAI4MBEAAAAAAjgxEQAAAAACODIRAAAAAAI4MxEAAAAAAjg0EQAAAAACODURAAAAAAI4NhEAAAAAAjg3EQAAAAACODgRAAAAAAI4OREAAAAAAjkwEQAAAAACOTERAAAAAAI5MhEAAAAAAjkzEQAAAAACOTQIAAAAAAI5NREAAAAAAjk3EQAAAAACOTgCAAAAAAI5OQ8AAAAAAzEwMREAAAAAAzEwMhEAAAAAAzEwNBEAAAAAAzEwNxEAAAAAAzEwOBEAAAAAAzEwOREAAAAAAzExNBEAAAAAAzExNQIAAAAAAzExNhEAAAAAAzExOBEAAAAAAzExOQ4AAAAAAzEyMAgAAAAAAzEyMQgAAAAAAzEyMg4AAAAAAzEyMwIAAAAAAzEyNAgAAAAAAzEyNREAAAAAAzEyNhEAAAAAAzEyOQgAAAAAAzEzMBEAAAAAAzEzMREAAAAAAzEzMhEAAAAAAzEzMxEAAAAAAzEzNA4AAAAAAzEzNREAAAAAAzEzNgIAAAAAAzEzNw8AAAAAAzEzOBEAAAAAAzEzOREAAAAAAzE0Mg8AAAAAAzE0MxEAAAAAAzE0NA4AAAAAAzE1MREAAAAAAzE2MAgAAAAAAzE2MRIAAAAAAzE2NQgAAAAAAzE3MBEAAAAAAzE3MREAAAAAAzMwMA4AAAAAAzMwMQ4AAAAAAzQ4MAgAAAAABDEzMDUOAAAAAAQ1MDQzCAAAAAAENTA0OAgAAAAABDUwNTARAAAAAAQ1MDUyEQAAAAAENTA1MxEAAAAABDU3MDARAAAAAAQ1NzUxAgAAAAAENTc1MgIAAAAABDU3NTMIAAAAAAQ1NzkwDwAAAAAENTc5MQ8AAAAABDU3OTIRAAAAAAQ1NzkzEQAAAAAENTgwMBEAAAAABDU4MDERAAAAAAQ1ODAyEQAAAAAENTgwMwIAAAAABDU4MDQRAAAAAAQ4MDAwEgAAAAAEOTAwMBEAAAAABDkwMDEIAAAAAAUxMjQwMAIAAAAABTEyNDAxEQAAAAAFMTI0MDIRAAAAAAUxMjQwMwgAAAAABTEyNDA0AgAAAAAFMTI0MzcIAAAAAAUxMjQzOAgAAAAABTEyNDQwAgAAAAAFMTI0NDEIAAAAAAUxMjQ0MhEAAAAABTEyNDQzCAAAAAAFMTI0NDQRAAAAAAUxMjQ0NREAAAAABTEyNDQ2CAAAAAAFMTI0NDcRAAAAAAUxMjQ3MBEAAAAABTEyNDcxDwAAAAAFMTI0NzIPAAAAAAUxMjQ3Mw8AAAAABTEyNDc0EQAAAAAFMTI0ODAOAAAAAAUxMjQ4NREAAAAABTEyNDg2AgAAAAAFMTI0OTARAAAAAAUxMjQ5MREAAAAABTEyNDk4AgAAAAAFMTI0OTkPAAAAAAQ1NzU0EQAAAAAENTc5NhEAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            //извлекаем из структуры NAV отражением и проверяем что записалось
            string datastr_correlated_1 = evaluate_data_set(mf_2_ds_c_0);
            //************************************

            service.InvokeApplicationMethod(objectType: ApplicationCodeType.FormCode, objectId: iam_s_fid_1, methodName: "NewRecord", dataSet: ref mf_2_ds_c_0, state: ref mf_2_s_c_0, args: ref iam_obj_1);

            //==============================================
            //  GetPage
            //NavRecordState gp_mf_2 = nofa_2_sf[0].State;
            service.GetPage(
                pageRequestDefinition: new PageRequestDefinition()
                {
                    CalcFields = new int[2] { 5801, 5802 },
                    IncludeMoreDataInformation = true,
                    IncludeNonRowData = true,
                    PageSize = 20,
                    PageSizeInOppositeDirection = 20,
                    ReadDirection = ReadDirection.Next,
                    StartFromPage = StartingPage.Specific,
                },
                state: ref sf_2_s_1_c_0
                );

            //==============================================
            //  lnvokeApplicationMethod
            //NavRecordState iam_s_2 = nofa_2_sf[0].State;
            //NavDataSet iam_nds_2 = nofa_2_sf[0].DataSet;
            int iam_s_fid_2 = nofa_2_sf[0].State.FormId;

            //************************************
            //       DATA FIELD CORRELATION
            //записываем в структуру NAV отражением
            sf_2_ds_1_c_1 = write_data_set("B2RhdGFTZXR/AAAAAQAAAA1QdXJjaGFzZSBMaW5lCQQAAOsAAAABMAoAAAAACGJvb2ttYXJrEwAAAAABMQgAAAAAATIRAAAAAAEzEQAAAAABNAgAAAAAATUIAAAAAAE2EQAAAAABNxEAAAAAATgRAAAAAAIxMA8AAAAAAjExEQAAAAACMTIRAAAAAAIxMxEAAAAAAjE1DgAAAAACMTYOAAAAAAIxNw4AAAAAAjE4DgAAAAACMjIOAAAAAAIyMw4AAAAAAjI1DgAAAAACMjcOAAAAAAIyOA4AAAAAAjI5DgAAAAACMzAOAAAAAAIzMQ4AAAAAAjMyAgAAAAACMzQOAAAAAAIzNQ4AAAAAAjM2DgAAAAACMzcOAAAAAAIzOAgAAAAAAjQwEQAAAAACNDERAAAAAAI0NREAAAAAAjU0DgAAAAACNTYCAAAAAAI1Nw4AAAAAAjU4DgAAAAACNTkOAAAAAAI2MA4AAAAAAjYxDgAAAAACNjMRAAAAAAI2NAgAAAAAAjY3DgAAAAACNjgRAAAAAAI2OQ4AAAAAAjcwEQAAAAACNzERAAAAAAI3MggAAAAAAjczAgAAAAACNzQRAAAAAAI3NREAAAAAAjc3CAAAAAACNzgRAAAAAAI3OREAAAAAAjgwCAAAAAACODERAAAAAAI4MhEAAAAAAjgzEQAAAAACODURAAAAAAI4NgIAAAAAAjg3EQAAAAACODgCAAAAAAI4OREAAAAAAjkwEQAAAAACOTERAAAAAAI5Mg4AAAAAAjkzDgAAAAACOTUOAAAAAAI5NxEAAAAAAjk4CAAAAAACOTkOAAAAAAMxMDAOAAAAAAMxMDECAAAAAAMxMDMOAAAAAAMxMDQOAAAAAAMxMDUOAAAAAAMxMDYRAAAAAAMxMDcIAAAAAAMxMDgRAAAAAAMxMDkOAAAAAAMxMTAOAAAAAAMxMTEOAAAAAAMxMTIOAAAAAAMxMTMOAAAAAAMxMTQOAAAAAAMxMTUOAAAAAAMxMTYIAAAAAAMxMTcRAAAAAAMxMTgRAAAAAAMxMTkCAAAAAAMxMjARAAAAAAMxMjEOAAAAAAMxMjIOAAAAAAMxMjMCAAAAAAMxMjQOAAAAAAMxMjkOAAAAAAMxMzARAAAAAAMxMzIOAAAAAAMxMzUOAAAAAAMxMzYOAAAAAAMxMzcOAAAAAAMxNDAOAAAAAAMxNDEOAAAAAAM0ODAIAAAAAAQxMDAxEQAAAAAEMTAwMggAAAAABDEwMDMOAAAAAAQxMDA0DgAAAAAEMTAwNQ4AAAAABDEwMDYOAAAAAAQxMDA3DgAAAAAEMTAwOA4AAAAABDEwMDkOAAAAAAQxMDEwDgAAAAAEMTAxMQ4AAAAABDEwMTIOAAAAAAQxMDEzEQAAAAAEMTAxOQgAAAAABDEwMzAOAAAAAAQxMDMxDgAAAAAEMTcwMBEAAAAABDE3MDIPAAAAAAQ1NDAxEQAAAAAENTQwMhEAAAAABDU0MDMRAAAAAAQ1NDA0DgAAAAAENTQwNxEAAAAABDU0MTUOAAAAAAQ1NDE2DgAAAAAENTQxNw4AAAAABDU0MTgOAAAAAAQ1NDU4DgAAAAAENTQ2MA4AAAAABDU0NjEOAAAAAAQ1NDk1DgAAAAAENTYwMA8AAAAABDU2MDEIAAAAAAQ1NjAyEQAAAAAENTYwMw4AAAAABDU2MDUCAAAAAAQ1NjA2AgAAAAAENTYwOREAAAAABDU2MTARAAAAAAQ1NjExEQAAAAAENTYxMhEAAAAABDU2MTMCAAAAAAQ1NzAwEQAAAAAENTcwNREAAAAABDU3MDYRAAAAAAQ1NzA3CAAAAAAENTcwOBEAAAAABDU3MDkRAAAAAAQ1NzEwAgAAAAAENTcxMREAAAAABDU3MTIRAAAAAAQ1NzEzAgAAAAAENTcxNBEAAAAABDU3MTUIAAAAAAQ1NzUwDgAAAAAENTc1MgIAAAAABDU3OTAPAAAAAAQ1NzkxDwAAAAAENTc5MhEAAAAABDU3OTMRAAAAAAQ1Nzk0DwAAAAAENTc5NQ8AAAAABDU4MDACAAAAAAQ1ODAxDgAAAAAENTgwMg4AAAAABDU4MDMOAAAAAAQ1ODA0DgAAAAAENTgwNQ4AAAAABDU4MDYOAAAAAAQ1ODA3DgAAAAAENTgwOA4AAAAABDU4MDkOAAAAAAQ1ODEwDgAAAAAENjYwMBEAAAAABDY2MDEIAAAAAAQ2NjA4EQAAAAAENjYwOQgAAAAABDY2MTACAAAAAAUxMjQwMBEAAAAABTEyNDAxCAAAAAAFMTI0MDIPAAAAAAUxMjQwMxEAAAAABTEyNDA0EQAAAAAFMTI0MDURAAAAAAUxMjQzMA4AAAAABTEyNDMxDgAAAAAFMTI0ODUCAAAAAAUxMjQ4NhEAAAAABTEyNDg3AgAAAAAFMTI0OTARAAAAAAUxNzMwMBEAAAAACDk5MDAwNzUwEQAAAAAIOTkwMDA3NTERAAAAAAg5OTAwMDc1MhEAAAAACDk5MDAwNzUzAgAAAAAIOTkwMDA3NTQIAAAAAAg5OTAwMDc1NQ4AAAAACDk5MDAwNzU2AgAAAAAIOTkwMDA3NTcIAAAAAAg5OTAwMDc1OBEAAAAACDk5MDAwNzU5CAAAAAAVQ29udHJvbDEyMTAwMzFfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwMzdfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwMzlfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNDFfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNDNfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNDVfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNDdfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNDlfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNTNfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNTdfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNTlfRm9ybWF0EQAAAAAOQ29udHJvbDEyMTAwOTERAAAAAA5Db250cm9sMTIxMDA5MxEAAAAADkNvbnRyb2wxMjEwMDk1EQAAAAAOQ29udHJvbDEyMTAwOTcRAAAAAA5Db250cm9sMTIxMDA5OREAAAAADkNvbnRyb2wxMjEwMTAxEQAAAAAdQ29udHJvbDEyMTAwMDdfRHluYW1pY0NhcHRpb24RAAAAAB1Db250cm9sMTIxMDAzN19EeW5hbWljQ2FwdGlvbhEAAAAAHUNvbnRyb2wxMjEwMDQ1X0R5bmFtaWNDYXB0aW9uEQAAAAAdQ29udHJvbDEyMTAwODdfRHluYW1pY0NhcHRpb24RAAAAAB1Db250cm9sMTIxMDA4OV9EeW5hbWljQ2FwdGlvbhEAAAAAHUNvbnRyb2wxMjEwMDkxX0R5bmFtaWNDYXB0aW9uEQAAAAAdQ29udHJvbDEyMTAwOTNfRHluYW1pY0NhcHRpb24RAAAAAB1Db250cm9sMTIxMDA5NV9EeW5hbWljQ2FwdGlvbhEAAAAAHUNvbnRyb2wxMjEwMDk3X0R5bmFtaWNDYXB0aW9uEQAAAAAdQ29udHJvbDEyMTAwOTlfRHluYW1pY0NhcHRpb24RAAAAAB1Db250cm9sMTIxMDEwMV9EeW5hbWljQ2FwdGlvbhEAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");
            //************************************

            service.InvokeApplicationMethod(objectType: ApplicationCodeType.FormCode, objectId: iam_s_fid_2, methodName: "NewRecord", dataSet: ref sf_2_ds_1_c_1, state: ref sf_2_s_1_c_1, args: ref iam_obj_1);

            //*************************************************
            //  ЗАПОЛНЕНИЕ ОТЧЕТА
            //*************************************************

            //==============================================
            //  Channel/RealAction ---------------- INSERT RECORD (!!!)SLSR(!!!)

            NavDataSet ir_poav = service.InsertRecord(state: ref mf_2_s_c_1, recDataSet: mf_2_ds_c_1, true, false);

            //***********************************************************            
            //  poav number correlation 

            var DataSet_poav = ir_poav.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var recdataset_poav = DataSet_poav.GetValue(ir_poav);
            byte[] databyte_1_poav = (byte[])recdataset_poav.GetType().GetProperty("Data").GetValue(recdataset_poav);

            for (int i = 0; i < databyte_1_poav.Length; i++)
            {
                if (databyte_1_poav.Skip(i).Take(poav_pattern.Length).SequenceEqual(poav_pattern))
                {
                    POAV_NUMBER = databyte_1_poav.Skip(i + 8 /*поав*/).ToArray().Take(9 /*-20-00419*/).ToArray();
                    POAV_NUMBER_lower_bookmark = databyte_1_poav.Skip(i + 8 /*поав*/).ToArray().Take(9 /*-20-00419*/).ToArray();
                    break;
                }
            }

            byte[] databyte_2_poav = returnDataByte(ir_poav);
            string datastr_2_poav = System.Text.Encoding.UTF8.GetString((byte[])databyte_2_poav);
            string poav_filter = Regex.Match(datastr_2_poav, "ПОАВ.{9}").ToString();

            //***********************************************************

            //==============================================
            //  GetMetadataForPageAndAllItsDependencies

            MasterPage gmfpaaid_3 = metadataService.GetMasterPage(
                pageId: 27,
                personalizationId: string.Empty,
                applyPersonalization: true
            );

            //==============================================
            //  OpenForm
            NavOpenFormArguments nofa_3_mf = new NavOpenFormArguments()
            {
                ControlId = null,

                State = new NavRecordState()
                {
                    CurrentRecord = null,
                    FormId = 27, //gmfpaaid_3.ID
                    //PersonalizationId = personalizationId2,
                    ParentFormId = 0,
                    NavFormEditable = false,
                    RenamingMode = RenamingMode.NoKeys,
                    TableView = new NavTableView()
                    {
                        Ascending = true,
                        TableId = 23,
                        SearchFilter = new NavFilterGroup()
                        {
                            FilterGroupNo = -1,
                            Filters = new NavFilter[0]
                        }
                    },
                    ValidateFieldsInOnNewRecord = true,
                    ValidateRequired = true
                }
            };
            NavOpenFormArguments[] nofa_3_sf = new List<NavOpenFormArguments>().Append(new NavOpenFormArguments()
            {
                ControlId = "1904651607",

                State = new NavRecordState()
                {
                    CurrentRecord = null,
                    FormId = 9094,
                    //PersonalizationId = personalizationId2,
                    ParentFormId = 27,
                    NavFormEditable = false,
                    RenamingMode = RenamingMode.NoKeys,
                    TableView = new NavTableView()
                    {
                        Ascending = true,
                        TableId = 23,
                        SearchFilter = new NavFilterGroup()
                        {
                            FilterGroupNo = -1,
                            Filters = new NavFilter[0]
                        }
                    },
                    ValidateFieldsInOnNewRecord = true,
                    ValidateRequired = true
                }
            }).Append(new NavOpenFormArguments()
            {
                ControlId = "1903435607",

                State = new NavRecordState()
                {
                    CurrentRecord = null,
                    FormId = 9095,
                    //PersonalizationId = personalizationId2,
                    ParentFormId = 27,
                    NavFormEditable = false,
                    RenamingMode = RenamingMode.NoKeys,
                    TableView = new NavTableView()
                    {
                        Ascending = true,
                        TableId = 23,
                        SearchFilter = new NavFilterGroup()
                        {
                            FilterGroupNo = -1,
                            Filters = new NavFilter[0]
                        }
                    },
                    ValidateFieldsInOnNewRecord = true,
                    ValidateRequired = true
                }
            }).ToArray();

            service.OpenForm(
                        mainForm: ref nofa_3_mf,
                        subForms: ref nofa_3_sf
                    );

            //***************************************************
            //FORM 2 STATES DUPLICATE COPY 
            NavRecordState mf_3_s_c_0 = nofa_3_mf.State;
            NavRecordState mf_3_s_c_1 = nofa_3_mf.State;
            NavRecordState mf_3_s_c_2 = nofa_3_mf.State;
            NavRecordState mf_3_s_c_3 = nofa_3_mf.State;
            NavRecordState mf_3_s_c_4 = nofa_3_mf.State;

            NavDataSet mf_3_ds_c_0 = nofa_3_mf.DataSet;
            NavDataSet mf_3_ds_c_1 = nofa_3_mf.DataSet;
            NavDataSet mf_3_ds_c_2 = nofa_3_mf.DataSet;
            NavDataSet mf_3_ds_c_3 = nofa_3_mf.DataSet;
            NavDataSet mf_3_ds_c_4 = nofa_3_mf.DataSet;

            NavRecordState sf_3_s_1_c_0 = nofa_3_sf[0].State;
            NavRecordState sf_3_s_1_c_1 = nofa_3_sf[0].State;
            NavRecordState sf_3_s_1_c_2 = nofa_3_sf[0].State;
            NavRecordState sf_3_s_1_c_3 = nofa_3_sf[0].State;
            NavRecordState sf_3_s_1_c_4 = nofa_3_sf[0].State;

            NavDataSet sf_3_ds_1_c_0 = nofa_3_sf[0].DataSet;
            NavDataSet sf_3_ds_1_c_1 = nofa_3_sf[0].DataSet;
            NavDataSet sf_3_ds_1_c_2 = nofa_3_sf[0].DataSet;
            NavDataSet sf_3_ds_1_c_3 = nofa_3_sf[0].DataSet;
            NavDataSet sf_3_ds_1_c_4 = nofa_3_sf[0].DataSet;

            NavRecordState sf_3_s_2_c_0 = nofa_3_sf[1].State;
            NavRecordState sf_3_s_2_c_1 = nofa_3_sf[1].State;
            NavRecordState sf_3_s_2_c_2 = nofa_3_sf[1].State;
            NavRecordState sf_3_s_2_c_3 = nofa_3_sf[1].State;
            NavRecordState sf_3_s_2_c_4 = nofa_3_sf[1].State;

            NavDataSet sf_3_ds_2_c_0 = nofa_3_sf[1].DataSet;
            NavDataSet sf_3_ds_2_c_1 = nofa_3_sf[1].DataSet;
            NavDataSet sf_3_ds_2_c_2 = nofa_3_sf[1].DataSet;
            NavDataSet sf_3_ds_2_c_3 = nofa_3_sf[1].DataSet;
            NavDataSet sf_3_ds_2_c_4 = nofa_3_sf[1].DataSet;
            //***************************************************


            //==============================================
            //  GetPage
            //NavRecordState gp_mf_3 = nofa_3_mf.State;
            service.GetPage(
                pageRequestDefinition: new PageRequestDefinition()
                {
                    CalcFields = new int[2] { 59, 67 },
                    IncludeMoreDataInformation = true,
                    IncludeNonRowData = true,
                    PageSize = 20,
                    PageSizeInOppositeDirection = 20,
                    ReadDirection = ReadDirection.Next,
                    StartFromPage = StartingPage.Specific,
                },
                state: ref mf_3_s_c_0
                );

            ////==============================================
            ////  GetPage
            //string gp_mf_4sb = "FwAAAAJ7BTYAMAAwADAAMAAAAAAA";

            ////NavRecordState gp_mf_4 = nofa_1_mf.State;
            //service.GetPage(
            //    pageRequestDefinition: new PageRequestDefinition()
            //    {
            //        IncludeMoreDataInformation = false,
            //        IncludeNonRowData = true,
            //        PageSize = 1,
            //        PageSizeInOppositeDirection = 0,
            //        ReadDirection = ReadDirection.Next,
            //        StartFromPage = StartingPage.Specific,
            //        StartingBookmark = Convert.FromBase64String(gp_mf_4sb)
            //    },
            //    state: ref mf_1_s_c_2
            //    );

            //a9("GetPage                   ", sw_25, sw_26, (short)14);

            ////==============================================
            ////  GetPage
            //NavRecordState gp_mf_5 = nofa_2_sf[0].State;
            //service.GetPage(
            //    pageRequestDefinition: new PageRequestDefinition()
            //    {
            //        CalcFields = new int[2] { 5801, 5802 },
            //        IncludeMoreDataInformation = true,
            //        IncludeNonRowData = true,
            //        PageSize = 20,
            //        PageSizeInOppositeDirection = 20,
            //        ReadDirection = ReadDirection.Next,
            //        StartFromPage = StartingPage.Specific,
            //    },
            //    state: ref sf_2_s_1_c_2
            //    );

            //a9("GetPage                   ", sw_26, sw_27, (short)14);

            ////==============================================
            ////  lnvokeApplicationMethod
            //service.InvokeApplicationMethod(objectType: ApplicationCodeType.FormCode, objectId: iam_s_fid_2, methodName: "NewRecord", dataSet: ref sf_2_ds_1_c_3, state: ref sf_2_s_1_c_3, args: ref iam_obj_1);

            //a9("lnvokeApplicationMethod   ", sw_27, sw_28, (short)4);

            ////==============================================
            ////  lnvokeApplicationMethod
            //service.InvokeApplicationMethod(objectType: ApplicationCodeType.FormCode, objectId: iam_s_fid_2, methodName: "NewRecord", dataSet: ref sf_2_ds_1_c_4, state: ref sf_2_s_1_c_4, args: ref iam_obj_1);

            //a9("lnvokeApplicationMethod   ", sw_28, sw_29, (short)4);

            ////==============================================
            ////  lnvokeApplicationMethod
            //service.InvokeApplicationMethod(objectType: ApplicationCodeType.FormCode, objectId: iam_s_fid_2, methodName: "NewRecord", dataSet: ref sf_2_ds_1_c_5, state: ref sf_2_s_1_c_5, args: ref iam_obj_1);

            //a9("lnvokeApplicationMethod   ", sw_29, sw_30, (short)4);

            //==============================================
            //  CloseForm
            {
                NavCloseFormArguments ncfa_3_mf = new NavCloseFormArguments()
                {
                    ExitAction = FormResult.LookupOK,
                    DataSet = nofa_3_mf.DataSet,
                    State = nofa_3_mf.State
                };

                NavCloseFormArguments[] ncfa_3_sf = nofa_3_sf
                    .Select(sf => new NavCloseFormArguments() { DataSet = sf.DataSet, State = sf.State, ExitAction = FormResult.None })
                    .ToArray();

                //************************************
                //       DATA FIELD CORRELATION
                ncfa_3_mf.DataSet = write_data_set("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AAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAIAAAAAAgAAAAACAAAAAAIAAAAAAgAAAAACAAAAAAIAAAAAAgAAAAACAAAAAAIAAAAAAgAAAAACAAAAAAIAAAAAAgAAAAACAAAAAAIAAAAAAgAAAAACAAAAAAIAAAAAAgAAAAACAAAAAAIAAAAAAgAAAAACAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAIAAgACAAIBAgACAAIAAggjLCMjMC4wMAIIIywjIzAuMDA=");
                //************************************

                service.CloseForm(
                    ref ncfa_3_mf,
                    ref ncfa_3_sf,
                    force: false);
            }












































            //==============================================
            //  ValidateField

            ////************************************
            ////       DATA FIELD CORRELATION
            //string datastr_original_2 = evaluate_data_set(mf_2_ds_c_2);

            //mf_2_ds_c_2 = write_data_set("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");

            //string datastr_original_2_1 = evaluate_data_set(mf_2_ds_c_2);

            //mf_2_ds_c_2 = correlate_data_field_COMPLEATE(mf_2_ds_c_2, POAV_NUMBER);

            //string datastr_correlated_2 = evaluate_data_set(mf_2_ds_c_2);
            ////************************************



            //byte[] cr_1 = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADAAMQAxADYAAAAAAA==");

            //string datastr_original70 = System.Text.Encoding.UTF8.GetString((byte[])cr_1);

            //correlate_poav_number(cr_1, POAV_NUMBER);

            //string datastr_original71 = System.Text.Encoding.UTF8.GetString((byte[])cr_1);


            //mf_2_s_c_2.CurrentRecord = cr_1;
            //mf_2_s_c_2.InsertLowerBoundBookmark = cr_1;
            //mf_2_s_c_2.InsertUpperBoundBookmark = cr_1;


            //service.ValidateField(state: ref mf_2_s_c_2, recDataSet: mf_2_ds_c_2, 1210014);

            ////a9("ValidateField", sw_1, sw_2);















            //string gp_mf_1_refresh_1 = "JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADAAMAA5ADgAAAAAAA==";

            //byte[] sb_1 = Convert.FromBase64String(gp_mf_1_refresh_1);

            //string datastr_original70 = System.Text.Encoding.UTF8.GetString((byte[])sb_1);

            //correlate_poav_number(sb_1, POAV_NUMBER);

            //string datastr_original71 = System.Text.Encoding.UTF8.GetString((byte[])sb_1);


            //NavRecordState asdad = gp_mf_1;

            //asdad.CurrentRecord = null;
            //asdad.FlushDataCache = true;
            //asdad.FormVariables = null;
            //asdad.MoreDataInReadDirection = false;
            //asdad.PageCaption = "";
            //asdad.PersonalizationId = "374f138d-58a0-4b24-b6f9-da451371b033";
            ////asdad.RecordState = (RecordState)"";
            //asdad.RenamingMode = RenamingMode.SingleKeyServerSide;


            //service.GetPage(
            //    pageRequestDefinition: new PageRequestDefinition()
            //    {
            //        ExcludeStartingRecord = false,
            //        IncludeMoreDataInformation = true,
            //        IncludeNonRowData = true,
            //        IsSubFormUpdateRequest = false,
            //        LookupFieldIds = null,
            //        LookupFieldValues = null,
            //        NormalFields = null,
            //        PageSize = 50,
            //        PageSizeInOppositeDirection = 50,
            //        ReadDirection = ReadDirection.Next,
            //        StartFromPage = StartingPage.Specific,
            //        StartingBookmark = sb_1
            //    },
            //    state: ref asdad
            //    );


            //string gp_mf_1_refresh_2 = "JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADAAMAA0ADUAAAAAAA==";

            //service.GetPage(
            //    pageRequestDefinition: new PageRequestDefinition()
            //    {
            //        ExcludeStartingRecord = true,
            //        IncludeMoreDataInformation = true,
            //        IncludeNonRowData = false,
            //        IsSubFormUpdateRequest = false,
            //        LookupFieldIds = null,
            //        LookupFieldValues = null,
            //        NormalFields = null,
            //        PageSize = 50,
            //        PageSizeInOppositeDirection = 0,
            //        ReadDirection = ReadDirection.Previous,
            //        StartFromPage = StartingPage.Specific,
            //        StartingBookmark = Convert.FromBase64String(gp_mf_1_refresh_2)
            //    },
            //    state: ref asdad
            //    );





















            //==============================================
            //  MODIFY RECORD
            ////mf_2_ds_c_3 = write_data_set("B2RhdGFTZXR/AAAAAQAAAA9QdXJjaGFzZSBIZWFkZXIJBAAAswAAAAEwCgAAAAAIYm9va21hcmsTAAAAAAExCAAAAAABMhEAAAAAATMRAAAAAAE0EQAAAAABNREAAAAAATYRAAAAAAE3EQAAAAABOBEAAAAAATkRAAAAAAIxMBEAAAAAAjExEQAAAAACMTIRAAAAAAIxMxEAAAAAAjE0EQAAAAACMTURAAAAAAIxNhEAAAAAAjE3EQAAAAACMTgRAAAAAAIxOQ8AAAAAAjIwDwAAAAACMjEPAAAAAAIyMhEAAAAAAjIzEQAAAAACMjQPAAAAAAIyNQ4AAAAAAjI2DwAAAAACMjcRAAAAAAIyOBEAAAAAAjI5EQAAAAACMzARAAAAAAIzMREAAAAAAjMyEQAAAAACMzMOAAAAAAIzNQIAAAAAAjM3EQAAAAACNDERAAAAAAI0MxEAAAAAAjQ1EQAAAAACNDYCAAAAAAI0NwgAAAAAAjUxEQAAAAACNTIIAAAAAAI1MxEAAAAAAjU1EQAAAAACNTYCAAAAAAI1NwIAAAAAAjU4AgAAAAACNTkCAAAAAAI2MA4AAAAAAjYxDgAAAAACNjIRAAAAAAI2MxEAAAAAAjY0EQAAAAACNjURAAAAAAI2NhEAAAAAAjY3EQAAAAACNjgRAAAAAAI2OREAAAAAAjcwEQAAAAACNzIRAAAAAAI3MxEAAAAAAjc0EQAAAAACNzYRAAAAAAI3NxEAAAAAAjc4EQAAAAACNzkRAAAAAAI4MBEAAAAAAjgxEQAAAAACODIRAAAAAAI4MxEAAAAAAjg0EQAAAAACODURAAAAAAI4NhEAAAAAAjg3EQAAAAACODgRAAAAAAI4OREAAAAAAjkwEQAAAAACOTERAAAAAAI5MhEAAAAAAjkzEQAAAAACOTQIAAAAAAI5NREAAAAAAjk3EQAAAAACOTgCAAAAAAI5OQ8AAAAAAzEwMREAAAAAAzEwMhEAAAAAAzEwNBEAAAAAAzEwNxEAAAAAAzEwOBEAAAAAAzEwOREAAAAAAzExNBEAAAAAAzExNQIAAAAAAzExNhEAAAAAAzExOBEAAAAAAzExOQ4AAAAAAzEyMAgAAAAAAzEyMQgAAAAAAzEyMg4AAAAAAzEyMwIAAAAAAzEyNAgAAAAAAzEyNREAAAAAAzEyNhEAAAAAAzEyOQgAAAAAAzEzMBEAAAAAAzEzMREAAAAAAzEzMhEAAAAAAzEzMxEAAAAAAzEzNA4AAAAAAzEzNREAAAAAAzEzNgIAAAAAAzEzNw8AAAAAAzEzOBEAAAAAAzEzOREAAAAAAzE0Mg8AAAAAAzE0MxEAAAAAAzE0NA4AAAAAAzE1MREAAAAAAzE2MAgAAAAAAzE2MRIAAAAAAzE2NQgAAAAAAzE3MBEAAAAAAzE3MREAAAAAAzMwMA4AAAAAAzMwMQ4AAAAAAzQ4MAgAAAAABDEzMDUOAAAAAAQ1MDQzCAAAAAAENTA0OAgAAAAABDUwNTARAAAAAAQ1MDUyEQAAAAAENTA1MxEAAAAABDU3MDARAAAAAAQ1NzUxAgAAAAAENTc1MgIAAAAABDU3NTMIAAAAAAQ1NzkwDwAAAAAENTc5MQ8AAAAABDU3OTIRAAAAAAQ1NzkzEQAAAAAENTgwMBEAAAAABDU4MDERAAAAAAQ1ODAyEQAAAAAENTgwMwIAAAAABDU4MDQRAAAAAAQ4MDAwEgAAAAAEOTAwMBEAAAAABDkwMDEIAAAAAAUxMjQwMAIAAAAABTEyNDAxEQAAAAAFMTI0MDIRAAAAAAUxMjQwMwgAAAAABTEyNDA0AgAAAAAFMTI0MzcIAAAAAAUxMjQzOAgAAAAABTEyNDQwAgAAAAAFMTI0NDEIAAAAAAUxMjQ0MhEAAAAABTEyNDQzCAAAAAAFMTI0NDQRAAAAAAUxMjQ0NREAAAAABTEyNDQ2CAAAAAAFMTI0NDcRAAAAAAUxMjQ3MBEAAAAABTEyNDcxDwAAAAAFMTI0NzIPAAAAAAUxMjQ3Mw8AAAAABTEyNDc0EQAAAAAFMTI0ODAOAAAAAAUxMjQ4NREAAAAABTEyNDg2AgAAAAAFMTI0OTARAAAAAAUxMjQ5MREAAAAABTEyNDk4AgAAAAAFMTI0OTkPAAAAAAQ1NzU0EQAAAAAENTc5NhEAAAAAAgAAAAIh+g0AAAAAAAIrAAAAJgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADAAMQAxADYAAAAAAAICAAAAAgACEdCf0J7QkNCSLTIxLTAwMTE2AgACAAIAAgACAAIAAgACAAIAAhpDUk9OVVMg0KDQvtGB0YHQuNGPINCX0JDQngIAAgvQoNC40L3QsywgNQIAAgzQnNC+0YHQutCy0LACAAIAQABTjcHYCAIAAAACAEAAU43B2AgCAAAAAgBAAFONwdgIAgAAAAIZSW52b2ljZSDQn9Ce0JDQki0yMS0wMDExNgIAAgAAAAAAAAAAAgAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAgAAAAIAAgACAAIAAgACAAIAAAAAAAAAAAAAAAAAAAAAAgACAAIAAgACAAIAAgAAAAACAAIAAAAAAgACAAIAAgACAAIAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIR0J/QntCQ0JItMjEtMDAxMTYCEdCf0J7QkNCSLTIxLTAwMTE2AgACAAIAAgACEdCf0J7QkNCSLTIxLTAwMTE2AgACAAIAAgACAAIAAgACAAIFNjIwMDACAAIAAgACAAIAAgACAAIAAgACAAIAAgYxMDMwNTQCAAICUlUCAAAAAAIAAgACAAIAQABTjcHYCAIAAAACAAIAAgACCdCf0J7Qmi0yMAIJ0J/QntCaLTIwAgnQn9Ce0JotMTUCAAIAAgACAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAACAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgACAAAAAAIAAgACAAAAAAIAAgACAAIAAgAAAAAAAAAAAAAAAAAAAAACAAIBAgAAAAAAAAAAAgAAAAIAAgACAAAAAAAAAAACAAAAAgACAAAAAAAAAAAAAAAAAAAAAAIAAgAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAgACAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAACAQAAAAIAAgACAAIAAgACAAIAAAAAAgAAAAAAAAAAAgAAAAIAAAAAAAAAAAIAAAACAAIAAgACAAIAAgACAAJ0KcFjvRsiQ7yAiDzkv2anAgACAAAAAAIBAgACAAIAAAAAAgACAAAAAAIAAAAAAgACAAAAAAIAAgAAAAACAAIAAgAAAAACAAIAAgAAAAAAAAAAAgAAAAIAAAAAAAAAAAIAAAACAAAAAAAAAAACAAAAAgACAAAAAAAAAAAAAAAAAAAAAAIAAgACAAIAAgACAAAAAAAAAAACAAAAAAACIfoNAAAAAAACKwAAACYAAAAAiwIAAAACe/8fBB4EEAQSBC0AMgAxAC0AMAAwADEAMQA2AAAAAAACAgAAAAIAAhHQn9Ce0JDQki0yMS0wMDExNgIAAgACAAIAAgACAAIAAgACAAIaQ1JPTlVTINCg0L7RgdGB0LjRjyDQl9CQ0J4CAAIL0KDQuNC90LMsIDUCAAIM0JzQvtGB0LrQstCwAgACAEAAU43B2AgCAAAAAgBAAFONwdgIAgAAAAIAQABTjcHYCAIAAAACGUludm9pY2Ug0J/QntCQ0JItMjEtMDAxMTYCAAIAAAAAAAAAAAIAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAIAAAACAAIAAgACAAIAAgACAAAAAAAAAAAAAAAAAAAAAAIAAgACAAIAAgACAAIAAAAAAgACAAAAAAIAAgACAAIAAgACAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACEdCf0J7QkNCSLTIxLTAwMTE2AhHQn9Ce0JDQki0yMS0wMDExNgIAAgACAAIAAhHQn9Ce0JDQki0yMS0wMDExNgIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACBjEwMzA1NAIAAgJSVQIAAAAAAgACAAIAAgBAAFONwdgIAgAAAAIAAgACAAIJ0J/QntCaLTIwAgnQn9Ce0JotMjACCdCf0J7Qmi0xNQIAAgACAAIAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAIAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAIAAAAAAgACAAIAAAAAAgACAAIAAgACAAAAAAAAAAAAAAAAAAAAAAIAAgECAAAAAAAAAAACAAAAAgACAAIAAAAAAAAAAAIAAAACAAIAAAAAAAAAAAAAAAAAAAAAAgACAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAACAAIAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAIBAAAAAgACAAIAAgACAAIAAgAAAAACAAAAAAAAAAACAAAAAgAAAAAAAAAAAgAAAAIAAgACAAIAAgACAAIAAnQpwWO9GyJDvICIPOS/ZqcCAAIAAAAAAgECAAIAAgAAAAACAAIAAAAAAgAAAAACAAIAAAAAAgACAAAAAAIAAgACAAAAAAIAAgACAAAAAAAAAAACAAAAAgAAAAAAAAAAAgAAAAIAAAAAAAAAAAIAAAACAAIAAAAAAAAAAAAAAAAAAAAAAgACAAIAAgACAAIAAAAAAAAAAAIAAAAAAA==");

            ////mf_2_ds_c_3 = correlate_data_field_COMPLEATE(mf_2_ds_c_3, POAV_NUMBER);


            //mf_2_ds_c_3 = correlate_data_filed_wrapper(
            //    mf_2_ds_c_3,
            //    POAV_NUMBER,
            //    "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"
            //    );
            ////************************************
            //byte[] cr_2 = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADAAMQAxADYAAAAAAA==");

            //correlate_poav_number(cr_2, POAV_NUMBER);

            ////mf_2_s_c_3.CurrentRecord = cr_2;
            ////mf_2_s_c_3.InsertLowerBoundBookmark = cr_2;
            ////mf_2_s_c_3.InsertUpperBoundBookmark = cr_2;
            //mf_2_s_c_3.CurrentRecord = null;
            //mf_2_s_c_3.InsertLowerBoundBookmark = null;
            //mf_2_s_c_3.InsertUpperBoundBookmark = null;
            //mf_2_s_c_3.RecordState = NavRecordOperationTypes.InDatabase;
            //mf_2_s_c_3.RenamingMode = RenamingMode.SingleKeyServerSide;
            //mf_2_s_c_3.SearchFilter = new NavFilterGroup()
            //{
            //    FilterGroupNo = -1,
            //    Filters = new NavFilter[0]
            //};



            //service.ModifyRecord(state: ref mf_2_s_c_3, recDataSet: mf_2_ds_c_3);
















            //a9("ModifyRecord", sw_1, sw_2);


            //==============================================
            //  GetPage

            //==============================================
            //  GetPage

            //==============================================
            //  lnvokeApplicationMethod
            //NavVisualizationHelper.Initialize(service);
            //service.InvokeApplicationMethod(objectType: ApplicationCodeType.FormCode, objectId: iam_s_fid_2, methodName: "NewRecord", dataSet: ref iam_nds_2, state: ref iam_s_2, args: ref iam_obj_1);


            //==============================================
            //  lnvokeApplicationMethod
            //NavVisualizationHelper.Initialize(service);
            //service.InvokeApplicationMethod(objectType: ApplicationCodeType.FormCode, objectId: iam_s_fid_2, methodName: "NewRecord", dataSet: ref iam_nds_2, state: ref iam_s_2, args: ref iam_obj_1);


            //==============================================
            //  lnvokeApplicationMethod
            //NavVisualizationHelper.Initialize(service);
            //service.InvokeApplicationMethod(objectType: ApplicationCodeType.FormCode, objectId: iam_s_fid_2, methodName: "NewRecord", dataSet: ref iam_nds_2, state: ref iam_s_2, args: ref iam_obj_1);



            //==============================================
            //  GetPage

            //==============================================
            //  ValidateField

            //==============================================
            //  INSERT RECORD

            //==============================================
            //  GetTableMetadata

            //==============================================
            //  GetMetadataForPageAndAllItsDependencies

            //==============================================
            //  OpenForm

            //==============================================
            //  GetPage

            //==============================================
            //  GetPage

            //==============================================
            //  CloseForm

            //==============================================
            //  Evaluate

            //==============================================
            //  ValidateField

            //==============================================
            //  Evaluate

            //==============================================
            //  ValidateField

            //==============================================
            //  MODIFY RECORD

            //==============================================
            //  CloseForm
            {
                NavCloseFormArguments ncfa_2_mf = new NavCloseFormArguments()
                {
                    ExitAction = FormResult.OK,
                    DataSet = nofa_2_mf.DataSet,
                    State = nofa_2_mf.State
                };

                NavCloseFormArguments[] ncfa_2_sf = nofa_2_sf
                    .Select(sf => new NavCloseFormArguments() { DataSet = sf.DataSet, State = sf.State, ExitAction = FormResult.None })
                    .ToArray();

                //************************************
                //       DATA FIELD CORRELATION
                string DataSet_str_cf_2 = "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AAACMjcOAAAAAAIyOA4AAAAAAjI5DgAAAAACMzAOAAAAAAIzMQ4AAAAAAjMyAgAAAAACMzQOAAAAAAIzNQ4AAAAAAjM2DgAAAAACMzcOAAAAAAIzOAgAAAAAAjQwEQAAAAACNDERAAAAAAI0NREAAAAAAjU0DgAAAAACNTYCAAAAAAI1Nw4AAAAAAjU4DgAAAAACNTkOAAAAAAI2MA4AAAAAAjYxDgAAAAACNjMRAAAAAAI2NAgAAAAAAjY3DgAAAAACNjgRAAAAAAI2OQ4AAAAAAjcwEQAAAAACNzERAAAAAAI3MggAAAAAAjczAgAAAAACNzQRAAAAAAI3NREAAAAAAjc3CAAAAAACNzgRAAAAAAI3OREAAAAAAjgwCAAAAAACODERAAAAAAI4MhEAAAAAAjgzEQAAAAACODURAAAAAAI4NgIAAAAAAjg3EQAAAAACODgCAAAAAAI4OREAAAAAAjkwEQAAAAACOTERAAAAAAI5Mg4AAAAAAjkzDgAAAAACOTUOAAAAAAI5NxEAAAAAAjk4CAAAAAACOTkOAAAAAAMxMDAOAAAAAAMxMDECAAAAAAMxMDMOAAAAAAMxMDQOAAAAAAMxMDUOAAAAAAMxMDYRAAAAAAMxMDcIAAAAAAMxMDgRAAAAAAMxMDkOAAAAAAMxMTAOAAAAAAMxMTEOAAAAAAMxMTIOAAAAAAMxMTMOAAAAAAMxMTQOAAAAAAMxMTUOAAAAAAMxMTYIAAAAAAMxMTcRAAAAAAMxMTgRAAAAAAMxMTkCAAAAAAMxMjARAAAAAAMxMjEOAAAAAAMxMjIOAAAAAAMxMjMCAAAAAAMxMjQOAAAAAAMxMjkOAAAAAAMxMzARAAAAAAMxMzIOAAAAAAMxMzUOAAAAAAMxMzYOAAAAAAMxMzcOAAAAAAMxNDAOAAAAAAMxNDEOAAAAAAM0ODAIAAAAAAQxMDAxEQAAAAAEMTAwMggAAAAABDEwMDMOAAAAAAQxMDA0DgAAAAAEMTAwNQ4AAAAABDEwMDYOAAAAAAQxMDA3DgAAAAAEMTAwOA4AAAAABDEwMDkOAAAAAAQxMDEwDgAAAAAEMTAxMQ4AAAAABDEwMTIOAAAAAAQxMDEzEQAAAAAEMTAxOQgAAAAABDEwMzAOAAAAAAQxMDMxDgAAAAAEMTcwMBEAAAAABDE3MDIPAAAAAAQ1NDAxEQAAAAAENTQwMhEAAAAABDU0MDMRAAAAAAQ1NDA0DgAAAAAENTQwNxEAAAAABDU0MTUOAAAAAAQ1NDE2DgAAAAAENTQxNw4AAAAABDU0MTgOAAAAAAQ1NDU4DgAAAAAENTQ2MA4AAAAABDU0NjEOAAAAAAQ1NDk1DgAAAAAENTYwMA8AAAAABDU2MDEIAAAAAAQ1NjAyEQAAAAAENTYwMw4AAAAABDU2MDUCAAAAAAQ1NjA2AgAAAAAENTYwOREAAAAABDU2MTARAAAAAAQ1NjExEQAAAAAENTYxMhEAAAAABDU2MTMCAAAAAAQ1NzAwEQAAAAAENTcwNREAAAAABDU3MDYRAAAAAAQ1NzA3CAAAAAAENTcwOBEAAAAABDU3MDkRAAAAAAQ1NzEwAgAAAAAENTcxMREAAAAABDU3MTIRAAAAAAQ1NzEzAgAAAAAENTcxNBEAAAAABDU3MTUIAAAAAAQ1NzUwDgAAAAAENTc1MgIAAAAABDU3OTAPAAAAAAQ1NzkxDwAAAAAENTc5MhEAAAAABDU3OTMRAAAAAAQ1Nzk0DwAAAAAENTc5NQ8AAAAABDU4MDACAAAAAAQ1ODAxDgAAAAAENTgwMg4AAAAABDU4MDMOAAAAAAQ1ODA0DgAAAAAENTgwNQ4AAAAABDU4MDYOAAAAAAQ1ODA3DgAAAAAENTgwOA4AAAAABDU4MDkOAAAAAAQ1ODEwDgAAAAAENjYwMBEAAAAABDY2MDEIAAAAAAQ2NjA4EQAAAAAENjYwOQgAAAAABDY2MTACAAAAAAUxMjQwMBEAAAAABTEyNDAxCAAAAAAFMTI0MDIPAAAAAAUxMjQwMxEAAAAABTEyNDA0EQAAAAAFMTI0MDURAAAAAAUxMjQzMA4AAAAABTEyNDMxDgAAAAAFMTI0ODUCAAAAAAUxMjQ4NhEAAAAABTEyNDg3AgAAAAAFMTI0OTARAAAAAAUxNzMwMBEAAAAACDk5MDAwNzUwEQAAAAAIOTkwMDA3NTERAAAAAAg5OTAwMDc1MhEAAAAACDk5MDAwNzUzAgAAAAAIOTkwMDA3NTQIAAAAAAg5OTAwMDc1NQ4AAAAACDk5MDAwNzU2AgAAAAAIOTkwMDA3NTcIAAAAAAg5OTAwMDc1OBEAAAAACDk5MDAwNzU5CAAAAAAVQ29udHJvbDEyMTAwMzFfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwMzdfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwMzlfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNDFfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNDNfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNDVfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNDdfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNDlfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNTNfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNTdfRm9ybWF0EQAAAAAVQ29udHJvbDEyMTAwNTlfRm9ybWF0EQAAAAAOQ29udHJvbDEyMTAwOTERAAAAAA5Db250cm9sMTIxMDA5MxEAAAAADkNvbnRyb2wxMjEwMDk1EQAAAAAOQ29udHJvbDEyMTAwOTcRAAAAAA5Db250cm9sMTIxMDA5OREAAAAADkNvbnRyb2wxMjEwMTAxEQAAAAAdQ29udHJvbDEyMTAwMDdfRHluYW1pY0NhcHRpb24RAAAAAB1Db250cm9sMTIxMDAzN19EeW5hbWljQ2FwdGlvbhEAAAAAHUNvbnRyb2wxMjEwMDQ1X0R5bmFtaWNDYXB0aW9uEQAAAAAdQ29udHJvbDEyMTAwODdfRHluYW1pY0NhcHRpb24RAAAAAB1Db250cm9sMTIxMDA4OV9EeW5hbWljQ2FwdGlvbhEAAAAAHUNvbnRyb2wxMjEwMDkxX0R5bmFtaWNDYXB0aW9uEQAAAAAdQ29udHJvbDEyMTAwOTNfRHluYW1pY0NhcHRpb24RAAAAAB1Db250cm9sMTIxMDA5NV9EeW5hbWljQ2FwdGlvbhEAAAAAHUNvbnRyb2wxMjEwMDk3X0R5bmFtaWNDYXB0aW9uEQAAAAAdQ29udHJvbDEyMTAwOTlfRHluYW1pY0NhcHRpb24RAAAAAB1Db250cm9sMTIxMDEwMV9EeW5hbWljQ2FwdGlvbhEAAAAAAQAAAAKvmg0AAAAAAAIrAAAAJgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADAAMAAzADQAAAAAAAICAAAAAgU2MjAwMAIR0J/QntCQ0JItMjEtMDAwMzQCBTYyMDAwAh/Ql9CQ0J4gItCh0L/QvtGA0YLQvNCw0YHRgtC10YAiAgACItGD0LsuINCR0L7RgtCw0L3QuNGH0LXRgdC60LDRjywgNDQCAAIM0JzQvtGB0LrQstCwAgACAAIAAhXQkdC10LvRi9C5INGB0LrQu9Cw0LQCAAIW0YPQuy4g0JvQtdC90LjQvdCwLCAzNAIAAh3QndC40LbQvdC40Lkg0J3QvtCy0LPQvtGA0L7QtAIAAgBAAFONwdgIAgAAAAIAQABTjcHYCAIAAAACAEAAU43B2AgCAAAAAhlJbnZvaWNlINCf0J7QkNCSLTIxLTAwMDM0AgTQndCfAgBAAFONwdgIAgAAAAIAAAAAAAAAAAAAAAAAAAAAAgBAAFONwdgIAgAAAAIS0KHQoi7QmCDQpNCg0JDQpdCiAgrQkdCV0JvQq9CZAgACAAIHNjAtMTAxMAIAAgAAAAAAAAAAAAAAAAAAAAACAAIFNjIwMDACAAIE0JLQlQIAAgACAAAAAAIAAgAAAAACAAIAAgACAAIAAgACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAhHQn9Ce0JDQki0yMS0wMDAzNAIR0J/QntCQ0JItMjEtMDAwMzQCAAIAAgACAAIR0J/QntCQ0JItMjEtMDAwMzQCAAIAAgACAAIM0JHQmNCX0J3QldChAgACAAIAAh/Ql9CQ0J4gItCh0L/QvtGA0YLQvNCw0YHRgtC10YAiAgACItGD0LsuINCR0L7RgtCw0L3QuNGH0LXRgdC60LDRjywgNDQCAAIM0JzQvtGB0LrQstCwAgACBjEwMzA1NAIAAgACBjEwMzA1NAIAAgACBjYwMzA2MQIAAgJSVQIAAAAAAgACAAIAAgBAAFONwdgIAgAAAAIAAgACAAIJ0J/QntCaLTIwAgnQn9Ce0JotMjACCdCf0J7Qmi0xNQIAAgACDtCf0J7QmtCj0J/QmtCQAgACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAgAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAgAAAAACAAIAAgAAAAACAAIAAgACAAIAAAAAAAAAAAAAAAAAAAAAAgACAQIAQABTjcHYCAIAAAACAAIAAgBAAFONwdgIAgAAAAIE0J3QnwIAAAAAAAAAAAAAAAAAAAAAAgACAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAACAAIAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIkAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAIBAAAAAgACCENUMDAwMzQ3AghDVDAwMDM0NwIM0JzQntCh0JrQktCQAgACAAIAAAAAAgAAAAAAAAAAAgAAAAIAAAAAAAAAAAIAAAACAAIAAgACAAIAAgACAAKs0IGOx9pzSZIoLF1gkGW/AgACAAAAAAIBAgACAAIAAAAAAgACAAAAAAIAAAAAAgACAAAAAAIAAgAAAAACAAIAAgAAAAACAAIAAgAAAAAAAAAAAgAAAAIAAAAAAAAAAAIAAAACAAAAAAAAAAACAAAAAgACAAAAAAAAAAAAAAAAAAAAAAIAAgACAAIAAgACAAAAAAAAAAACAAAAAAABAAAAArGaDQAAAAAAAjEAAAAnAAAAAIsCAAAAAnv/HwQeBBAEEgQtADIAMQAtADAAMAAwADMANAAAAACHECcAAAAAAgIAAAACBTYyMDAwAhHQn9Ce0JDQki0yMS0wMDAzNAIQJwAAAgIAAAACCtCi0J7Qki0wMTACCtCh0JjQndCY0JkCBzQxLTEwMDACAEAAU43B2AgCAAAAAjTQmNC90YHRgi4g0YHRgtC+0LsgItCy0LXRh9C90YvQuSDQutCw0LvQtdC90LTQsNGA0YwiAgACCtCo0YLRg9C60LACZAAAAAAAAAAAAAAAAAAAAAJkAAAAAAAAAAAAAAAAAAAAAmQAAAAAAAAAAAAAAAAAAAACZAAAAAAAAAAAAAAAAAAAAAKN7wIAAAAAAAAAAAAAAAIAAo3vAgAAAAAAAAAAAAAAAgACFAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACFJMlAQAAAAAAAAAAAAACAAIYSmABAAAAAAAAAAAAAAIAAlw1AAAAAAAAAAAAAAAAAAACAQIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAACAAIAAgACAAAAAAAAAAAAAAAAAAAAAAIBAhhKYAEAAAAAAAAAAAAAAgACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAgAAAAACAAAAAAAAAAAAAAAAAAAAAAIFNjIwMDACAAAAAAAAAAAAAAAAAAAAAAIAAgACAAAAAAIAAgzQkdCY0JfQndCV0KECCNCi0J7QkjIwAgAAAAACAAIAAgAAAAACAAIAAgACAAIAAgACAAIO0J/QntCa0KPQn9Ca0JACCNCi0J7QkjIwAgACGEpgAQAAAAAAAAAAAAACAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAIAAAAAAhSTJQEAAAAAAAAAAAAAAgACje8CAAAAAAAAAAAAAAACAAIAAhSTJQEAAAAAAAAAAAAAAgACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgjQotCe0JIyMAIAAAAAAgACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAgACAAIAAgACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIUkyUBAAAAAAAAAAAAAAIAAgAAAAAAAAAAAAAAAAAAAAACJAAAAAIAAgAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAgAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgACAAAAAAAAAAACAAAAAgACAAIAAgEAAAAAAAAAAAAAAAAAAAACBNCo0KICgJaYAAAAAAAAAAAAAAAFAAKAlpgAAAAAAAAAAAAAAAUAAoCWmAAAAAAAAAAAAAAABQACgJaYAAAAAAAAAAAAAAAFAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAgAAAAIAAAAAAgACAAAAAAAAAAAAAAAAAAAAAAIAAgACAAIAAgACAAIAAgzQnNCe0KHQmtCS0JACAAIAAgAAAAACAAIAAgACAAIAAgACAAIAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAIAAAAAAAAAAAIAAAACAAAAAAAAAAACAAAAAgACAAIAgJYoxMDYCAIAAAACAICWKMTA2AgCAAAAAgECAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAgAAAAACAAIAAAAAAgACAAIAAAAAAgAAAAAAAAAAAgAAAAIAAgACAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAIAAgACAAIAAgACAAIAAgACAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgACAAAAAAICMUQCAAAAAAILIywjIzAuIyMjIyMCCyMsIyMwLjAwIyMjAgsjLCMjMC4jIyMjIwILIywjIzAuMDAjIyMCCyMsIyMwLjAwIyMjAggjLCMjMC4wMAILIywjIzAuIyMjIyMCCCMsIyMwLjAwAggjLCMjMC4wMAILIywjIzAuIyMjIyMCCyMsIyMwLiMjIyMjAAAAAAAAAgNOby4CGkRpcmVjdCBVbml0IENvc3QgRXhjbC4gVkFUAhVMaW5lIEFtb3VudCBFeGNsLiBWQVQCD0RlcGFydG1lbnQgQ29kZQIV0JTQvtGF0YDQsNGB0YUg0JrQvtC0AgxQcm9qZWN0IENvZGUCEkN1c3RvbWVyZ3JvdXAgQ29kZQIJQXJlYSBDb2RlAhJCdXNpbmVzc2dyb3VwIENvZGUCEtCd0YMt0LLQuNC0INCa0L7QtAIY0J3Rgy3QvtCx0YrQtdC60YIg0JrQvtC0";
                byte[] DataSet_cf_2 = Convert.FromBase64String(DataSet_str_cf_2);

                NavDataSet NavDataSet_cf_2 = new NavDataSet { DataSetName = "Purchase Header" };

                var datasetdata_cf_2 = NavDataSet_cf_2.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(NavDataSet_cf_2);
                datasetdata_cf_2.GetType().GetProperty("Data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(datasetdata_cf_2, DataSet_cf_2);
                NavDataSet_cf_2.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(NavDataSet_cf_2, datasetdata_cf_2);

                ncfa_2_mf.DataSet = NavDataSet_cf_2;
                //************************************

                service.CloseForm(
                    ref ncfa_2_mf,
                    ref ncfa_2_sf,
                    force: false);
            }


            ////*************************************************
            ////  ВСТАВКА В БД
            ////*************************************************
            //SetConsoleTextAttribute(GetStdHandle(-11), (short)62); Console.WriteLine("****** sql insert to database ******");


            //string queryString = "select [No_], [Pay-to Name] from [dbo].[CRONUS Россия ЗАО$Purchase Header];";
            //using (SqlConnection connection = new SqlConnection("Data Source=WIN-OCNJUHB1LAG; Initial Catalog=NAVTEST; Integrated Security=True;"))
            //{
            //    SqlCommand command = new SqlCommand(queryString, connection);
            //    connection.Open();
            //    using (SqlDataReader reader = command.ExecuteReader())
            //    {
            //        while (reader.Read())
            //        {
            //            Console.WriteLine(String.Format("{0}, {1}", reader[0], reader[1]));
            //        }
            //    }
            //}

            //UPDATE [dbo].[CRONUS Россия ЗАО$Purchase Header] set[Buy-from Vendor No_] = '62000', [Pay-to Vendor No_] = '62000', [Pay-to Name] = N'ЗАО "Спортмастер"', [Pay-to Address] = N'ул. Ботаническая, 44', [Pay-to City] = N'Москва', [Ship-to Name] = N'Белый склад', [Ship-to Address] = N'ул. Ленина, 34', [Ship-to City] = N'Нижний Новгород', [Payment Terms Code] = N'НП', [Due Date] = CAST(N'2021-01-26T00:00:00.000' AS DateTime), [Pmt_ Discount Date] = CAST(N'2021-01-26T00:00:00.000' AS DateTime), [Shipment Method Code] = N'СТ.И ФРАХТ', [Location Code] = N'БЕЛЫЙ', [Vendor Posting Group] = '60-1010', [Invoice Disc_ Code] = '62000', [Purchaser Code] = N'ВЕ', [Gen_ Bus_ Posting Group] = N'БИЗНЕС', [Buy-from Vendor Name] = N'ЗАО "Спортмастер"', [Buy-from Address] = N'ул. Ботаническая, 44', [Buy-from City] = N'Москва', [Pay-to Post Code] = '103054', [Buy-from Post Code] = '103054', [Ship-to Post Code] = '603061', [VAT Bus_ Posting Group] = N'ПОКУПКА', [Prepayment Due Date] = CAST(N'2021-01-26T00:00:00.000' AS DateTime), [Prepmt_ Pmt_ Discount Date] = CAST(N'2021-01-26T00:00:00.000' AS DateTime), [Prepmt_ Payment Terms Code] = N'НП', [Dimension Set ID] = '36', [Buy-from Contact No_] = 'CT000347', [Pay-to Contact No_] = 'CT000347', [Responsibility Center] = N'МОСКВА'where No_ = 'ПОАВ-21-00065'; 
            //            INSERT[dbo].[CRONUS Россия ЗАО$Purchase Line]([Document Type], [Document No_], [Line No_], [Buy-from Vendor No_], [Type], [No_], [Location Code], [Posting Group], [Expected Receipt Date], [Description], [Description 2], [Unit of Measure], [Quantity], [Outstanding Quantity], [Qty_ to Invoice], [Qty_ to Receive], [Direct Unit Cost], [Unit Cost(LCY)], [VAT _], [Line Discount _], [Line Discount Amount], [Amount], [Amount Including VAT], [Unit Price(LCY)], [Allow Invoice Disc_], [Gross Weight], [Net Weight], [Units per Parcel], [Unit Volume], [Appl_-to Item Entry], [Shortcut Dimension 1 Code], [Shortcut Dimension 2 Code], [Job No_], [Indirect Cost _], [Recalculate Invoice Disc_], [Outstanding Amount], [Qty_ Rcd_ Not Invoiced], [Amt_ Rcd_ Not Invoiced], [Quantity Received], [Quantity Invoiced], [Receipt No_], [Receipt Line No_], [Profit _], [Pay - to Vendor No_], [Inv_ Discount Amount], [Vendor Item No_], [Sales Order No_], [Sales Order Line No_], [Drop Shipment], [Gen_ Bus_ Posting Group], [Gen_ Prod_ Posting Group], [VAT Calculation Type], [Transaction Type], [Transport Method], [Attached to Line No_], [Entry Point], [Area], [Transaction Specification], [Tax Area Code], [Tax Liable], [Tax Group Code], [Use Tax], [VAT Bus_ Posting Group], [VAT Prod_ Posting Group], [Currency Code], [Outstanding Amount(LCY)], [Amt_ Rcd_ Not Invoiced(LCY)], [Blanket Order No_], [Blanket Order Line No_], [VAT Base Amount], [Unit Cost], [System - Created Entry], [Line Amount], [VAT Difference], [Inv_ Disc_ Amount to Invoice], [VAT Identifier], [IC Partner Ref_ Type], [IC Partner Reference], [Prepayment _], [Prepmt_ Line Amount], [Prepmt_ Amt_ Inv_], [Prepmt_ Amt_ Incl_ VAT], [Prepayment Amount], [Prepmt_ VAT Base Amt_], [Prepayment VAT _], [Prepmt_ VAT Calc_ Type], [Prepayment VAT Identifier], [Prepayment Tax Area Code], [Prepayment Tax Liable], [Prepayment Tax Group Code], [Prepmt Amt to Deduct], [Prepmt Amt Deducted], [Prepayment Line], [Prepmt_ Amount Inv_ Incl_ VAT], [Prepmt_ Amount Inv_(LCY)], [IC Partner Code], [Prepmt_ VAT Amount Inv_(LCY)], [Prepayment VAT Difference], [Prepmt VAT Diff_ to Deduct], [Prepmt VAT Diff_ Deducted], [Outstanding Amt_ Ex_ VAT(LCY)], [A_ Rcd_ Not Inv_ Ex_ VAT(LCY)], [Dimension Set ID], [Job Task No_], [Job Line Type], [Job Unit Price], [Job Total Price], [Job Line Amount], [Job Line Discount Amount], [Job Line Discount _], [Job Unit Price(LCY)], [Job Total Price(LCY)], [Job Line Amount(LCY)], [Job Line Disc_ Amount(LCY)], [Job Currency Factor], [Job Currency Code], [Job Planning Line No_], [Job Remaining Qty_], [Job Remaining Qty_(Base)], [Deferral Code], [Returns Deferral Start Date], [Prod_ Order No_], [Variant Code], [Bin Code], [Qty_ per Unit of Measure], [Unit of Measure Code], [Quantity(Base)], [Outstanding Qty_(Base)], [Qty_ to Invoice(Base)], [Qty_ to Receive(Base)], [Qty_ Rcd_ Not Invoiced(Base)], [Qty_ Received(Base)], [Qty_ Invoiced(Base)], [FA Posting Date], [FA Posting Type], [Depreciation Book Code], [Salvage Value], [Depr_ until FA Posting Date], [Depr_ Acquisition Cost], [Maintenance Code], [Insurance No_], [Budgeted FA No_], [Duplicate in Depreciation Book], [Use Duplication List], [Responsibility Center], [Cross - Reference No_], [Unit of Measure(Cross Ref_)], [Cross - Reference Type], [Cross - Reference Type No_], [Item Category Code], [Nonstock], [Purchasing Code], [Product Group Code], [Special Order], [Special Order Sales No_], [Special Order Sales Line No_], [Completely Received], [Requested Receipt Date], [Promised Receipt Date], [Lead Time Calculation],[Inbound Whse_ Handling Time], [Planned Receipt Date], [Order Date], [Allow Item Charge Assignment], [Return Qty_ to Ship], [Return Qty_ to Ship(Base)], [Return Qty_ Shipped Not Invd_], [Ret_ Qty_ Shpd Not Invd_(Base)], [Return Shpd_ Not Invd_], [Return Shpd_ Not Invd_(LCY)], [Return Qty_ Shipped], [Return Qty_ Shipped(Base)], [Return Shipment No_], [Return Shipment Line No_], [Return Reason Code], [Subtype], [Copied From Posted Doc_], [FA Location Code], [Empl_ Purchase Entry No_], [Empl_ Purchase Document Date], [Empl_ Purchase Vendor No_], [Empl_ Purchase Document No_], [Employee No_], [Amount(LCY)], [Amount Including VAT(LCY)], [No Ledger Entry], [FA Charge No_], [Surplus], [Agreement No_], [Tax Difference Code], [Routing No_], [Operation No_], [Work Center No_], [Finished], [Prod_ Order Line No_], [Overhead Rate], [MPS Order], [Planning Flexibility], [Safety Lead Time], [Routing Reference No_]) 
            //VALUES('2', N'ПОАВ-21-00065', '10000', '62000', '2', N'ТОВ-010', N'СИНИЙ', '41-1000', CAST(N'2021-01-26T00:00:00.000' AS DateTime), N'Инст. стол "вечный календарь"', N'', N'Штука', CAST(100.00000000000000000000 AS Decimal(38, 20)), CAST(100.00000000000000000000 AS Decimal(38, 20)), CAST(100.00000000000000000000 AS Decimal(38, 20)), CAST(100.00000000000000000000 AS Decimal(38, 20)), CAST(1923.97000000000000000000 AS Decimal(38, 20)), CAST(1923.97000000000000000000 AS Decimal(38, 20)), CAST(20.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(192397.00000000000000000000 AS Decimal(38, 20)), CAST(230876.40000000000000000000 AS Decimal(38, 20)), CAST(13660.00000000000000000000 AS Decimal(38, 20)), '1', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), '0', N'', N'', N'', CAST(0.00000000000000000000 AS Decimal(38, 20)), '1', CAST(230876.40000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), N'', '0', CAST(0.00000000000000000000 AS Decimal(38, 20)), '62000', CAST(0.00000000000000000000 AS Decimal(38, 20)), N'', N'', '0', '0', N'БИЗНЕС', N'ТОВ20', '0', N'', N'', '0', N'', N'', N'', N'', '0', N'', '0', N'ПОКУПКА', N'ТОВ20', N'', CAST(230876.40000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), N'', '0', CAST(192397.00000000000000000000 AS Decimal(38, 20)), CAST(1923.97000000000000000000 AS Decimal(38, 20)), '0', CAST(192397.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), N'ТОВ20', '0', N'', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), '0', N'', N'', '0', N'', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), '0', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), N'', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(192397.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), '36', N'', '0', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), N'', '0', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), N'', CAST(N'1753-01-01T00:00:00.000' AS DateTime), N'', N'', N'', '1.00000000000000000000', N'ШТ', CAST(100.00000000000000000000 AS Decimal(38, 20)), CAST(100.00000000000000000000 AS Decimal(38, 20)), CAST(100.00000000000000000000 AS Decimal(38, 20)), CAST(100.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(N'1753-01-01T00:00:00.000' AS DateTime), '0', N'', CAST(0.00000000000000000000 AS Decimal(38, 20)), '0', '0', N'', N'', N'', N'', '0', N'МОСКВА', N'', N'', '0', N'', N'', '0', N'', N'', '0', N'', '0', '0', CAST(N'1753-01-01T00:00:00.000' AS DateTime), CAST(N'1753-01-01T00:00:00.000' AS DateTime), N'', N'', CAST(N'2021-01-25T00:00:00.000' AS DateTime), CAST(N'2021-01-25T00:00:00.000' AS DateTime), '1', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), N'', '0', N'', '0', '0', N'', '0', CAST(N'1753-01-01T00:00:00.000' AS DateTime), N'', N'', N'', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), '0', N'', '0', N'', N'', N'', N'', N'', '0', '0', CAST(0.00000000000000000000 AS Decimal(38, 20)), '0', '0', '1', '0');


            //======================================================
            // ПРОВЕРКА КОННЕКТА К БД
            string SQL_q1 = "UPDATE [dbo].[CRONUS Россия ЗАО$Purchase Header] set[Buy-from Vendor No_] = '62000', [Pay-to Vendor No_] = '62000', [Pay-to Name] = N'ЗАО \"Спортмастер\"', [Pay-to Address] = N'ул. Ботаническая, 44', [Pay-to City] = N'Москва', [Ship-to Name] = N'Белый склад', [Ship-to Address] = N'ул. Ленина, 34', [Ship-to City] = N'Нижний Новгород', [Payment Terms Code] = N'НП', [Due Date] = CAST(N'2021-01-26T00:00:00.000' AS DateTime), [Pmt_ Discount Date] = CAST(N'2021-01-26T00:00:00.000' AS DateTime), [Shipment Method Code] = N'СТ.И ФРАХТ', [Location Code] = N'БЕЛЫЙ', [Vendor Posting Group] = '60-1010', [Invoice Disc_ Code] = '62000', [Purchaser Code] = N'ВЕ', [Gen_ Bus_ Posting Group] = N'БИЗНЕС', [Buy-from Vendor Name] = N'ЗАО \"Спортмастер\"', [Buy-from Address] = N'ул. Ботаническая, 44', [Buy-from City] = N'Москва', [Pay-to Post Code] = '103054', [Buy-from Post Code] = '103054', [Ship-to Post Code] = '603061', [VAT Bus_ Posting Group] = N'ПОКУПКА', [Prepayment Due Date] = CAST(N'2021-01-26T00:00:00.000' AS DateTime), [Prepmt_ Pmt_ Discount Date] = CAST(N'2021-01-26T00:00:00.000' AS DateTime), [Prepmt_ Payment Terms Code] = N'НП', [Dimension Set ID] = '36', [Buy-from Contact No_] = 'CT000347', [Pay-to Contact No_] = 'CT000347', [Responsibility Center] = N'МОСКВА'where No_ = 'ПОАВ-21-00065'; ";
            string SQL_q2 = "INSERT [dbo].[CRONUS Россия ЗАО$Purchase Line] ([Document Type], [Document No_], [Line No_], [Buy-from Vendor No_], [Type], [No_], [Location Code], [Posting Group], [Expected Receipt Date], [Description], [Description 2], [Unit of Measure], [Quantity], [Outstanding Quantity], [Qty_ to Invoice], [Qty_ to Receive], [Direct Unit Cost], [Unit Cost (LCY)], [VAT _], [Line Discount _], [Line Discount Amount], [Amount], [Amount Including VAT], [Unit Price (LCY)], [Allow Invoice Disc_], [Gross Weight], [Net Weight], [Units per Parcel], [Unit Volume], [Appl_-to Item Entry], [Shortcut Dimension 1 Code], [Shortcut Dimension 2 Code], [Job No_], [Indirect Cost _], [Recalculate Invoice Disc_], [Outstanding Amount], [Qty_ Rcd_ Not Invoiced] , [Amt_ Rcd_ Not Invoiced], [Quantity Received], [Quantity Invoiced], [Receipt No_], [Receipt Line No_], [Profit _], [Pay-to Vendor No_], [Inv_ Discount Amount], [Vendor Item No_], [Sales Order No_], [Sales Order Line No_], [Drop Shipment], [Gen_ Bus_ Posting Group], [Gen_ Prod_ Posting Group], [VAT Calculation Type], [Transaction Type], [Transport Method], [Attached to Line No_], [Entry Point], [Area], [Transaction Specification], [Tax Area Code], [Tax Liable], [Tax Group Code], [Use Tax], [VAT Bus_ Posting Group], [VAT Prod_ Posting Group], [Currency Code], [Outstanding Amount (LCY)], [Amt_ Rcd_ Not Invoiced (LCY)], [Blanket Order No_], [Blanket Order Line No_], [VAT Base Amount], [Unit Cost], [System-Created Entry], [Line Amount], [VAT Difference], [Inv_ Disc_ Amount to Invoice], [VAT Identifier], [IC Partner Ref_ Type], [IC Partner Reference], [Prepayment _], [Prepmt_ Line Amount], [Prepmt_ Amt_ Inv_], [Prepmt_ Amt_ Incl_ VAT], [Prepayment Amount], [Prepmt_ VAT Base Amt_], [Prepayment VAT _], [Prepmt_ VAT Calc_ Type], [Prepayment VAT Identifier], [Prepayment Tax Area Code], [Prepayment Tax Liable], [Prepayment Tax Group Code], [Prepmt Amt to Deduct], [Prepmt Amt Deducted], [Prepayment Line], [Prepmt_ Amount Inv_ Incl_ VAT], [Prepmt_ Amount Inv_ (LCY)], [IC Partner Code], [Prepmt_ VAT Amount Inv_ (LCY)], [Prepayment VAT Difference], [Prepmt VAT Diff_ to Deduct], [Prepmt VAT Diff_ Deducted], [Outstanding Amt_ Ex_ VAT (LCY)], [A_ Rcd_ Not Inv_ Ex_ VAT (LCY)], [Dimension Set ID], [Job Task No_], [Job Line Type], ";
            string SQL_q3 = "[Job Unit Price], [Job Total Price], [Job Line Amount], [Job Line Discount Amount], [Job Line Discount _], [Job Unit Price (LCY)], [Job Total Price (LCY)], [Job Line Amount (LCY)], [Job Line Disc_ Amount (LCY)], [Job Currency Factor], [Job Currency Code], [Job Planning Line No_], [Job Remaining Qty_], [Job Remaining Qty_ (Base)], [Deferral Code], [Returns Deferral Start Date], [Prod_ Order No_], [Variant Code], [Bin Code], [Qty_ per Unit of Measure], [Unit of Measure Code], [Quantity (Base)], [Outstanding Qty_ (Base)], [Qty_ to Invoice (Base)], [Qty_ to Receive (Base)], [Qty_ Rcd_ Not Invoiced (Base)], [Qty_ Received (Base)], [Qty_ Invoiced (Base)], [FA Posting Date], [FA Posting Type], [Depreciation Book Code], [Salvage Value], [Depr_ until FA Posting Date], [Depr_ Acquisition Cost], [Maintenance Code], [Insurance No_], [Budgeted FA No_], [Duplicate in Depreciation Book], [Use Duplication List], [Responsibility Center], [Cross-Reference No_], [Unit of Measure (Cross Ref_)], [Cross-Reference Type], [Cross-Reference Type No_], [Item Category Code], [Nonstock], [Purchasing Code], [Product Group Code], [Special Order], [Special Order Sales No_], [Special Order Sales Line No_], [Completely Received], [Requested Receipt Date], [Promised Receipt Date], [Lead Time Calculation],[Inbound Whse_ Handling Time], [Planned Receipt Date], [Order Date], [Allow Item Charge Assignment], [Return Qty_ to Ship], [Return Qty_ to Ship (Base)], [Return Qty_ Shipped Not Invd_], [Ret_ Qty_ Shpd Not Invd_(Base)], [Return Shpd_ Not Invd_], [Return Shpd_ Not Invd_ (LCY)], [Return Qty_ Shipped], [Return Qty_ Shipped (Base)], [Return Shipment No_], [Return Shipment Line No_], [Return Reason Code], [Subtype], [Copied From Posted Doc_], [FA Location Code], [Empl_ Purchase Entry No_], [Empl_ Purchase Document Date], [Empl_ Purchase Vendor No_], [Empl_ Purchase Document No_], [Employee No_], [Amount (LCY)], [Amount Including VAT (LCY)], [No Ledger Entry], [FA Charge No_], [Surplus], [Agreement No_], [Tax Difference Code], [Routing No_], [Operation No_], [Work Center No_], [Finished], [Prod_ Order Line No_], [Overhead Rate], [MPS Order], [Planning Flexibility], [Safety Lead Time], [Routing Reference No_]) ";
            string SQL_q4 = "VALUES ('2',N'ПОАВ-21-00065','10000','62000','2',N'ТОВ-010',N'СИНИЙ','41-1000',CAST(N'2021-01-26T00:00:00.000' AS DateTime),N'Инст. стол \"вечный календарь\"',N'',N'Штука',CAST(100.00000000000000000000 AS Decimal(38, 20)),CAST(100.00000000000000000000 AS Decimal(38, 20)),CAST(100.00000000000000000000 AS Decimal(38, 20)),CAST(100.00000000000000000000 AS Decimal(38, 20)),CAST(1923.97000000000000000000 AS Decimal(38, 20)),CAST(1923.97000000000000000000 AS Decimal(38, 20)),CAST(20.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),CAST(192397.00000000000000000000 AS Decimal(38, 20)),CAST(230876.40000000000000000000 AS Decimal(38, 20)),CAST(13660.00000000000000000000 AS Decimal(38, 20)),'1', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),'0',N'',N'',N'', CAST(0.00000000000000000000 AS Decimal(38, 20)),'1',CAST(230876.40000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),N'','0', CAST(0.00000000000000000000 AS Decimal(38, 20)),'62000', CAST(0.00000000000000000000 AS Decimal(38, 20)),N'',N'','0','0',N'БИЗНЕС',N'ТОВ20','0',N'',N'','0',N'',N'',N'',N'','0',N'','0',N'ПОКУПКА',N'ТОВ20',N'',CAST(230876.40000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),N'','0',CAST(192397.00000000000000000000 AS Decimal(38, 20)),CAST(1923.97000000000000000000 AS Decimal(38, 20)),'0',CAST(192397.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),N'ТОВ20','0',N'', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),'0',N'',N'','0',N'', ";
            string SQL_q5 = "CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),'0', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),N'', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),CAST(192397.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),'36',N'','0', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),N'','0', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),N'',CAST(N'1753-01-01T00:00:00.000' AS DateTime),N'',N'',N'','1.00000000000000000000',N'ШТ',CAST(100.00000000000000000000 AS Decimal(38, 20)),CAST(100.00000000000000000000 AS Decimal(38, 20)),CAST(100.00000000000000000000 AS Decimal(38, 20)),CAST(100.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),CAST(N'1753-01-01T00:00:00.000' AS DateTime),'0',N'', CAST(0.00000000000000000000 AS Decimal(38, 20)),'0','0',N'',N'',N'',N'','0',N'МОСКВА',N'',N'','0',N'',N'','0',N'',N'','0',N'','0','0',CAST(N'1753-01-01T00:00:00.000' AS DateTime),CAST(N'1753-01-01T00:00:00.000' AS DateTime),N'',N'',CAST(N'2021-01-25T00:00:00.000' AS DateTime),CAST(N'2021-01-25T00:00:00.000' AS DateTime),'1', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),N'','0',N'','0','0',N'','0',CAST(N'1753-01-01T00:00:00.000' AS DateTime),N'',N'',N'', CAST(0.00000000000000000000 AS Decimal(38, 20)), CAST(0.00000000000000000000 AS Decimal(38, 20)),'0',N'','0',N'',N'',N'',N'',N'','0','0', CAST(0.00000000000000000000 AS Decimal(38, 20)),'0','0','1','0');";
            string queryString = SQL_q1 + SQL_q2 + SQL_q3 + SQL_q4 + SQL_q5;

            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = "Data Source=WIN-OCNJUHB1LAG; Initial Catalog=NAVTEST; Integrated Security=True;";
                SqlCommand command = new SqlCommand(queryString.Replace("ПОАВ-21-00065", poav_filter), connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {

                    }
                }
                finally
                {
                    reader.Close();
                }
            }



            ////*************************************************
            ////  ОБНОВИТЬ СТРАНИЦУ С ОТЧЕТАМИ
            ////*************************************************

            //==============================================
            //  GetPage

            mf_1_s_c_5.CurrentRecord = null;
            mf_1_s_c_5.InsertLowerBoundBookmark = null;
            mf_1_s_c_5.InsertUpperBoundBookmark = null;
            mf_1_s_c_5.FlushDataCache = true;
            mf_1_s_c_5.PersonalizationId = "374f138d-58a0-4b24-b6f9-da451371b033";
            mf_1_s_c_5.RecordState = NavRecordOperationTypes.InDatabase;
            mf_1_s_c_5.RenamingMode = RenamingMode.SingleKeyServerSide;
            mf_1_s_c_5.SearchFilter = new NavFilterGroup()
            {
                FilterGroupNo = -1,
                Filters = new NavFilter[0]
            };

            service.GetPage(
                pageRequestDefinition: new PageRequestDefinition()
                {
                    ExcludeStartingRecord = false,
                    IncludeMoreDataInformation = true,
                    IncludeNonRowData = true,
                    IsSubFormUpdateRequest = false,
                    LookupFieldIds = null,
                    LookupFieldValues = null,
                    NormalFields = null,
                    PageSize = 50,
                    PageSizeInOppositeDirection = 50,
                    ReadDirection = ReadDirection.Next,
                    StartFromPage = StartingPage.Specific,
                    StartingBookmark = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADMAMQA1ADMAAAAAAA==")
                },
                state: ref mf_1_s_c_5
                );

            //==============================================
            //  GetPage

            mf_1_s_c_6.CurrentRecord = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADAALQAwADAAOAA3ADUAAAAAAA==");
            mf_1_s_c_6.InsertLowerBoundBookmark = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADAALQAwADAAOAA3ADIAAAAAAA==");
            mf_1_s_c_6.InsertUpperBoundBookmark = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADAALQAwADAAOAA3ADUAAAAAAA==");
            mf_1_s_c_6.FlushDataCache = true;
            mf_1_s_c_6.PersonalizationId = "374f138d-58a0-4b24-b6f9-da451371b033";
            mf_1_s_c_6.RecordState = NavRecordOperationTypes.InDatabase;
            mf_1_s_c_6.RenamingMode = RenamingMode.SingleKeyServerSide;
            mf_1_s_c_6.SearchFilter = new NavFilterGroup()
            {
                FilterGroupNo = -1,
                Filters = new NavFilter[0]
            };

            service.GetPage(
                pageRequestDefinition: new PageRequestDefinition()
                {
                    ExcludeStartingRecord = true,
                    IncludeMoreDataInformation = true,
                    IncludeNonRowData = false,
                    IsSubFormUpdateRequest = false,
                    LookupFieldIds = null,
                    LookupFieldValues = null,
                    NormalFields = null,
                    PageSize = 50,
                    PageSizeInOppositeDirection = 0,
                    ReadDirection = ReadDirection.Previous,
                    StartFromPage = StartingPage.Specific,
                    StartingBookmark = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADMAMAA5ADEAAAAAAA==")
                },
                state: ref mf_1_s_c_6
                );


















            System.Guid nofa_4_sfh = new Guid("00000000-0000-0000-0000-000000000000");

            //==============================================
            //  OpenForm
            NavOpenFormArguments nofa_4_mf = new NavOpenFormArguments()
            {
                ControlId = null,

                State = new NavRecordState()
                {
                    //AllSelected = false,
                    //AutoKeyValues = null,
                    //Changes = null,
                    //ClientRecordDraft = false,
                    //CurrentFilterGroup = 0,
                    CurrentRecord = correlate_poav_number_wrapper_string("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADMAMQA1ADUAAAAAAA==", POAV_NUMBER),
                    //FlushDataCache = false,
                    FormId = 12431,
                    //FormVariables = null,
                    //InsertLowerBoundBookmark = null,
                    //InsertUpperBoundBookmark = null,
                    //IsResourceDefinedForm = false,
                    //IsSubFormUpdateRequest = false,
                    //MoreDataInOppositeDirection = false,
                    //MoreDataInReadDirection = false,
                    NavFormEditable = true,
                    //NonSelectedRecords = null,
                    //PageCaption = null,
                    ParentFormId = 0,
                    //PersonalizationId = null,
                    //RecordTemporary = false,
                    RenamingMode = RenamingMode.SingleKeyServerSide,
                    //RunFormOnRec = true,
                    //SelectedRecords = null,
                    //ServerFormHandle = nofa_4_sfh,
                    //SubFormSelectionStates = null,
                    //SubFormUpdateRequests = null,
                    TableView = new NavTableView()
                    {
                        Ascending = true,
                        TableId = 38,
                        CurrentSortingFieldIds = new int[2] { 1, 3 },
                        SearchFilter = new NavFilterGroup()
                        {
                            FilterGroupNo = -1,
                            Filters = new NavFilter[0]
                        },
                        CurrentFilters = new NavFilterGroup[1] {
                            new NavFilterGroup() {
                                FilterGroupNo = 2,
                                Filters = new NavFilter[2]{
                                    new NavFilter(){
                                        FilterField = 1,
                                        FilterValue = "2",
                                        IsExactValue = true,
                                        OptionsAsCaptionsFilterValue = "Invoice",
                                        UserTypedFilterValue = null
                                    },
                                    new NavFilter(){
                                        FilterField = 12400,
                                        FilterValue = "1",
                                        IsExactValue = true,
                                        OptionsAsCaptionsFilterValue = "Yes",
                                        UserTypedFilterValue = null
                                    }
                                }
                            }
                        }
                    },
                    //Timeout = 0,
                    //UpdatePropagation = false,
                    ValidateFieldsInOnNewRecord = true,
                    ValidateRequired = true
                }
            };
            NavOpenFormArguments[] nofa_4_sf = new List<NavOpenFormArguments>().Append(new NavOpenFormArguments()
            {
                ControlId = "1210001",

                State = new NavRecordState()
                {
                    //AllSelected = false,
                    //AutoKeyValues = null,
                    //Changes = null,
                    //ClientRecordDraft = false,
                    //CurrentFilterGroup = 0,
                    CurrentRecord = null,
                    //FlushDataCache = false,
                    FormId = 12432,
                    //FormVariables = null,
                    //InsertLowerBoundBookmark = null,
                    //InsertUpperBoundBookmark = null,
                    //IsResourceDefinedForm = false,
                    //IsSubFormUpdateRequest = false,
                    //MoreDataInOppositeDirection = false,
                    //MoreDataInReadDirection = false,
                    NavFormEditable = true,
                    //NonSelectedRecords = null,
                    //PageCaption = null,
                    ParentFormId = 12431,
                    //PersonalizationId = null,
                    //RecordTemporary = false,
                    RenamingMode = RenamingMode.NoKeys,
                    //RunFormOnRec = false,
                    //SelectedRecords = null,
                    //ServerFormHandle = nofa_4_sfh,
                    //SubFormUpdateRequests = null,
                    TableView = new NavTableView()
                    {
                        Ascending = true,
                        TableId = 39,
                        SearchFilter = new NavFilterGroup()
                        {
                            FilterGroupNo = -1,
                            Filters = new NavFilter[0]
                        },
                        CurrentFilters = new NavFilterGroup[2] {
                            new NavFilterGroup() {
                                FilterGroupNo = 4,
                                Filters = new NavFilter[1]{
                                    new NavFilter(){
                                        FilterField = 3,
                                        FilterValue = poav_filter,
                                        IsExactValue = true,
                                        OptionsAsCaptionsFilterValue = "",
                                        UserTypedFilterValue = null
                                    }
                                }
                            },
                            new NavFilterGroup() {
                                FilterGroupNo = 2,
                                Filters = new NavFilter[1]{
                                    new NavFilter(){
                                        FilterField = 1,
                                        FilterValue = "2",
                                        IsExactValue = true,
                                        OptionsAsCaptionsFilterValue = "Счет",
                                        UserTypedFilterValue = null
                                    }
                                }
                            }
                        }
                    },
                    //Timeout = 0,
                    //UpdatePropagation = false,
                    ValidateFieldsInOnNewRecord = true,
                    ValidateRequired = true
                }
            }).ToArray();

            service.OpenForm(
                        mainForm: ref nofa_4_mf,
                        subForms: ref nofa_4_sf
                    );








            NavRecordState mf_4_s_1_0 = nofa_4_mf.State;
            NavRecordState mf_4_s_1_1 = nofa_4_mf.State;
            NavRecordState sf_4_s_1_0 = nofa_4_sf[0].State;
            NavRecordState sf_4_s_1_1 = nofa_4_sf[0].State;


            mf_4_s_1_0.CurrentRecord = null;
            mf_4_s_1_0.RecordState = NavRecordOperationTypes.InDatabase;
            mf_4_s_1_0.RenamingMode = RenamingMode.SingleKeyServerSide;
            mf_4_s_1_0.SearchFilter = new NavFilterGroup() { FilterGroupNo = -1, Filters = new NavFilter[0] };

            service.GetPage(
                pageRequestDefinition: new PageRequestDefinition()
                {
                    CalcFields = null,
                    ExcludeStartingRecord = false,
                    IncludeMoreDataInformation = true,
                    IncludeNonRowData = true,
                    IsSubFormUpdateRequest = false,
                    LookupFieldIds = null,
                    LookupFieldValues = null,
                    NormalFields = null,
                    PageSize = 1,
                    PageSizeInOppositeDirection = 0,
                    ReadDirection = ReadDirection.Next,
                    StartFromPage = StartingPage.Specific,
                    StartingBookmark = correlate_poav_number_wrapper_string("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADMAMQA1ADUAAAAAAA==", POAV_NUMBER)
                },
                state: ref mf_4_s_1_0
                );

            mf_4_s_1_1.CurrentRecord = null;
            mf_4_s_1_1.RecordState = NavRecordOperationTypes.InDatabase;
            mf_4_s_1_1.RenamingMode = RenamingMode.SingleKeyServerSide;
            mf_4_s_1_1.SearchFilter = new NavFilterGroup() { FilterGroupNo = -1, Filters = new NavFilter[0] };

            service.GetPage(
                pageRequestDefinition: new PageRequestDefinition()
                {
                    CalcFields = null,
                    ExcludeStartingRecord = false,
                    IncludeMoreDataInformation = true,
                    IncludeNonRowData = true,
                    IsSubFormUpdateRequest = false,
                    LookupFieldIds = null,
                    LookupFieldValues = null,
                    NormalFields = null,
                    PageSize = 1,
                    PageSizeInOppositeDirection = 0,
                    ReadDirection = ReadDirection.Next,
                    StartFromPage = StartingPage.Specific,
                    StartingBookmark = correlate_poav_number_wrapper_string("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADMAMQA1ADUAAAAAAA==", POAV_NUMBER)
                },
                state: ref mf_4_s_1_1
                );

            sf_4_s_1_0.CurrentRecord = null;
            sf_4_s_1_0.RecordState = NavRecordOperationTypes.InDatabase;
            sf_4_s_1_0.SearchFilter = new NavFilterGroup() { FilterGroupNo = -1, Filters = new NavFilter[0] };

            service.GetPage(
                pageRequestDefinition: new PageRequestDefinition()
                {
                    CalcFields = new int[2] { 5801, 5802 },
                    ExcludeStartingRecord = false,
                    IncludeMoreDataInformation = true,
                    IncludeNonRowData = true,
                    IsSubFormUpdateRequest = false,
                    LookupFieldIds = null,
                    LookupFieldValues = null,
                    NormalFields = null,
                    PageSize = 20,
                    PageSizeInOppositeDirection = 20,
                    ReadDirection = ReadDirection.Next,
                    StartFromPage = StartingPage.Specific,
                    StartingBookmark = null
                },
                state: ref sf_4_s_1_0
                );

            sf_4_s_1_1.CurrentRecord = null;
            sf_4_s_1_1.RecordState = NavRecordOperationTypes.InDatabase;
            sf_4_s_1_1.SearchFilter = new NavFilterGroup() { FilterGroupNo = -1, Filters = new NavFilter[0] };

            service.GetPage(
                pageRequestDefinition: new PageRequestDefinition()
                {
                    CalcFields = new int[2] { 5801, 5802 },
                    ExcludeStartingRecord = false,
                    IncludeMoreDataInformation = true,
                    IncludeNonRowData = true,
                    IsSubFormUpdateRequest = false,
                    LookupFieldIds = null,
                    LookupFieldValues = null,
                    NormalFields = null,
                    PageSize = 20,
                    PageSizeInOppositeDirection = 20,
                    ReadDirection = ReadDirection.Next,
                    StartFromPage = StartingPage.Specific,
                    StartingBookmark = correlate_poav_number_wrapper_string("JwAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADMAMQA1ADUAAAAAhxAnAAAAAA==", POAV_NUMBER)
                },
                state: ref sf_4_s_1_1
                );










            {
                NavCloseFormArguments ncfa_4_mf = new NavCloseFormArguments()
                {
                    ExitAction = FormResult.OK,
                    DataSet = nofa_4_mf.DataSet,
                    State = nofa_4_mf.State
                };

                NavCloseFormArguments[] ncfa_4_sf = nofa_4_sf
                    .Select(sf => new NavCloseFormArguments() { DataSet = sf.DataSet, State = sf.State, ExitAction = FormResult.None })
                    .ToArray();

                //************************************
                //       DATA FIELD CORRELATION
                //string DataSet_str_cf_4 = "Ck5ld0RhdGFTZXR/AAAAAgAAAAEwGQQAALMAAAAIYm9va21hcmsTAAAAAAEwCgAAAAABMQgAAAAAATMRAAAAAAEyEQAAAAABNBEAAAAAATURAAAAAAE2EQAAAAABNxEAAAAAATgRAAAAAAE5EQAAAAACMTARAAAAAAIxMREAAAAAAjEyEQAAAAACMTMRAAAAAAIxNBEAAAAAAjE1EQAAAAACMTYRAAAAAAIxNxEAAAAAAjE4EQAAAAACMTkPAAAAAAIyMA8AAAAAAjIxDwAAAAACMjIRAAAAAAIyMxEAAAAAAjI0DwAAAAACMjUOAAAAAAIyNg8AAAAAAjI3EQAAAAACMjgRAAAAAAIyOREAAAAAAjMwEQAAAAACMzERAAAAAAIzMhEAAAAAAjMzDgAAAAACMzUCAAAAAAIzNxEAAAAAAjQxEQAAAAACNDMRAAAAAAI0NREAAAAAAjQ3CAAAAAACNTERAAAAAAI1MggAAAAAAjUzEQAAAAACNTURAAAAAAI1NwIAAAAAAjU4AgAAAAACNTkCAAAAAAI2MhEAAAAAAjYzEQAAAAACNjQRAAAAAAI2NREAAAAAAjY2EQAAAAACNjcRAAAAAAI2OBEAAAAAAjY5EQAAAAACNzARAAAAAAI3MhEAAAAAAjczEQAAAAACNzQRAAAAAAI3NhEAAAAAAjc3EQAAAAACNzgRAAAAAAI3OREAAAAAAjgwEQAAAAACODERAAAAAAI4MhEAAAAAAjgzEQAAAAACODQRAAAAAAI4NREAAAAAAjg2EQAAAAACODcRAAAAAAI4OBEAAAAAAjg5EQAAAAACOTARAAAAAAI5MREAAAAAAjkyEQAAAAACOTMRAAAAAAI5NAgAAAAAAjk1EQAAAAACOTcRAAAAAAI5OAIAAAAAAjk5DwAAAAADMTAxEQAAAAADMTAyEQAAAAADMTA0EQAAAAADMTA3EQAAAAADMTA4EQAAAAADMTA5EQAAAAADMTE0EQAAAAADMTE1AgAAAAADMTE2EQAAAAADMTE4EQAAAAADMTE5DgAAAAADMTIwCAAAAAADMTIxCAAAAAADMTIyDgAAAAADMTIzAgAAAAADMTI0CAAAAAADMTI1EQAAAAADMTI2EQAAAAADMTI5CAAAAAADMTMwEQAAAAADMTMxEQAAAAADMTMyEQAAAAADMTMzEQAAAAADMTM0DgAAAAADMTM1EQAAAAADMTM2AgAAAAADMTM3DwAAAAADMTM4EQAAAAADMTM5EQAAAAADMTQyDwAAAAADMTQzEQAAAAADMTQ0DgAAAAADMTUxEQAAAAADMTYwCAAAAAADMTYxEgAAAAADMTY1CAAAAAADMTcwEQAAAAADMTcxEQAAAAADNDgwCAAAAAAENTA0OAgAAAAABDUwNTARAAAAAAQ1MDUyEQAAAAAENTA1MxEAAAAABDU3MDARAAAAAAQ1NzUzCAAAAAAENTc5MA8AAAAABDU3OTEPAAAAAAQ1NzkyEQAAAAAENTc5MxEAAAAABDU4MDARAAAAAAQ1ODAxEQAAAAAENTgwMhEAAAAABDU4MDMCAAAAAAQ1ODA0EQAAAAAEODAwMBIAAAAABDkwMDARAAAAAAUxMjQwMAIAAAAABTEyNDAxEQAAAAAFMTI0MDIRAAAAAAUxMjQwMwgAAAAABTEyNDA0AgAAAAAFMTI0MzcIAAAAAAUxMjQzOAgAAAAABTEyNDQwAgAAAAAFMTI0NDEIAAAAAAUxMjQ0MhEAAAAABTEyNDQzCAAAAAAFMTI0NDQRAAAAAAUxMjQ0NREAAAAABTEyNDQ2CAAAAAAFMTI0NDcRAAAAAAUxMjQ3MBEAAAAABTEyNDcxDwAAAAAFMTI0NzIPAAAAAAUxMjQ3Mw8AAAAABTEyNDc0EQAAAAAFMTI0ODURAAAAAAUxMjQ4NgIAAAAABTEyNDkwEQAAAAAFMTI0OTERAAAAAAUxMjQ5OAIAAAAABTEyNDk5DwAAAAAENTc1NBEAAAAABDU3OTYPAAAAAAI0NgIAAAAAAjU2AgAAAAACNjAOAAAAAAI2MQ4AAAAAAzMwMA4AAAAAAzMwMQ4AAAAABDEzMDUOAAAAAAQ1MDQzCAAAAAAENTc1MQIAAAAABDU3NTICAAAAAAQ5MDAxCAAAAAAFMTI0ODAOAAAAAAExGQQAAOAAAAAIYm9va21hcmsTAAAAAAEwCgAAAAABMQgAAAAAATMRAAAAAAE0CAAAAAABMhEAAAAAATUIAAAAAAE2EQAAAAABNxEAAAAAATgRAAAAAAIxMA8AAAAAAjExEQAAAAACMTIRAAAAAAIxMxEAAAAAAjE1DgAAAAACMTYOAAAAAAIxNw4AAAAAAjE4DgAAAAACMjIOAAAAAAIyMw4AAAAAAjI1DgAAAAACMjcOAAAAAAIyOA4AAAAAAjI5DgAAAAACMzAOAAAAAAIzMQ4AAAAAAjMyAgAAAAACMzQOAAAAAAIzNQ4AAAAAAjM2DgAAAAACMzcOAAAAAAIzOAgAAAAAAjQwEQAAAAACNDERAAAAAAI0NREAAAAAAjU0DgAAAAACNTYCAAAAAAI1Nw4AAAAAAjU4DgAAAAACNTkOAAAAAAI2MA4AAAAAAjYxDgAAAAACNjMRAAAAAAI2NAgAAAAAAjY3DgAAAAACNjgRAAAAAAI2OQ4AAAAAAjcwEQAAAAACNzERAAAAAAI3MggAAAAAAjczAgAAAAACNzQRAAAAAAI3NREAAAAAAjc3CAAAAAACNzgRAAAAAAI3OREAAAAAAjgwCAAAAAACODERAAAAAAI4MhEAAAAAAjgzEQAAAAACODURAAAAAAI4NgIAAAAAAjg3EQAAAAACODgCAAAAAAI4OREAAAAAAjkwEQAAAAACOTERAAAAAAI5Mg4AAAAAAjkzDgAAAAACOTcRAAAAAAI5OAgAAAAAAjk5DgAAAAADMTAwDgAAAAADMTAxAgAAAAADMTAzDgAAAAADMTA0DgAAAAADMTA1DgAAAAADMTA2EQAAAAADMTA3CAAAAAADMTA4EQAAAAADMTA5DgAAAAADMTEwDgAAAAADMTExDgAAAAADMTEyDgAAAAADMTEzDgAAAAADMTE0DgAAAAADMTE1DgAAAAADMTE2CAAAAAADMTE3EQAAAAADMTE4EQAAAAADMTE5AgAAAAADMTIwEQAAAAADMTIxDgAAAAADMTIyDgAAAAADMTIzAgAAAAADMTI0DgAAAAADMTI5DgAAAAADMTMwEQAAAAADMTMyDgAAAAADMTM1DgAAAAADMTM2DgAAAAADMTM3DgAAAAADMTQwDgAAAAADMTQxDgAAAAADNDgwCAAAAAAEMTAwMREAAAAABDEwMDIIAAAAAAQxMDAzDgAAAAAEMTAwNA4AAAAABDEwMDUOAAAAAAQxMDA2DgAAAAAEMTAwNw4AAAAABDEwMDgOAAAAAAQxMDA5DgAAAAAEMTAxMA4AAAAABDEwMTEOAAAAAAQxMDEyDgAAAAAEMTAxMxEAAAAABDEwMTkIAAAAAAQxMDMwDgAAAAAEMTAzMQ4AAAAABDE3MDARAAAAAAQxNzAyDwAAAAAENTQwMREAAAAABDU0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AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAIAAgACAAIAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACje8CAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAiQAAAACAAIAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAIAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAgAAAAAAAAAAAgAAAAIAAgACAAIBAAAAAAAAAAAAAAAAAAAAAgTQqNCiAmQAAAAAAAAAAAAAAAAAAAACZAAAAAAAAAAAAAAAAAAAAAJkAAAAAAAAAAAAAAAAAAAAAmQAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAACAAAAAgAAAAACAAIAAAAAAAAAAAAAAAAAAAAAAgACAAIAAgACAAIAAgACDNCc0J7QodCa0JLQkAIAAgACAAAAAAIAAgACAAIAAgACAAIAAgAAAAACAAIAAAAAAAAAAAIAAAACAAAAAAAAAAACAAAAAgACAAIAgJYoxMDYCAIAAAACAICWKMTA2AgCAAAAAgECAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgACAAAAAAIAAgAAAAACAAIAAgAAAAACAAAAAAAAAAACAAAAAgACAAIAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAgACAAIAAgACAAIAAgACAAIAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAIAAAAAAgMx0JQCAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACCyMsIyMwLiMjIyMjAgsjLCMjMC4wMCMjIwILIywjIzAuIyMjIyMCCyMsIyMwLjAwIyMjAgsjLCMjMC4wMCMjIwIIIywjIzAuMDACCyMsIyMwLiMjIyMjAggjLCMjMC4wMAIIIywjIzAuMDACCyMsIyMwLiMjIyMjAgsjLCMjMC4jIyMjIwAAAAAAAA==";
                //byte[] DataSet_cf_4 = Convert.FromBase64String(DataSet_str_cf_4);

                //NavDataSet NavDataSet_cf_4 = new NavDataSet { DataSetName = "Purchase Header" };

                //var datasetdata_cf_4 = NavDataSet_cf_4.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(NavDataSet_cf_4);
                //datasetdata_cf_4.GetType().GetProperty("Data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(datasetdata_cf_4, DataSet_cf_4);
                //NavDataSet_cf_4.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(NavDataSet_cf_4, datasetdata_cf_4);

                //ncfa_4_mf.DataSet = NavDataSet_cf_4;

                ncfa_4_mf.DataSet = correlate_data_filed_wrapper(
                    ncfa_4_mf.DataSet,
                    POAV_NUMBER,
                    "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"
                    );
                //************************************

                service.CloseForm(
                    ref ncfa_4_mf,
                    ref ncfa_4_sf,
                    force: false);
            }























            ////*************************************************
            ////  ОБНОВИТЬ СТРАНИЦУ С ОТЧЕТАМИ
            ////*************************************************

            //==============================================
            //  GetPage

            mf_1_s_c_8.CurrentRecord = null;
            mf_1_s_c_8.InsertLowerBoundBookmark = null;
            mf_1_s_c_8.InsertUpperBoundBookmark = null;
            mf_1_s_c_8.FlushDataCache = true;
            mf_1_s_c_8.PersonalizationId = "374f138d-58a0-4b24-b6f9-da451371b033";
            mf_1_s_c_8.RecordState = NavRecordOperationTypes.InDatabase;
            mf_1_s_c_8.RenamingMode = RenamingMode.SingleKeyServerSide;
            mf_1_s_c_8.SearchFilter = new NavFilterGroup()
            {
                FilterGroupNo = -1,
                Filters = new NavFilter[0]
            };


            service.GetPage(
                pageRequestDefinition: new PageRequestDefinition()
                {
                    ExcludeStartingRecord = false,
                    IncludeMoreDataInformation = true,
                    IncludeNonRowData = true,
                    IsSubFormUpdateRequest = false,
                    LookupFieldIds = null,
                    LookupFieldValues = null,
                    NormalFields = null,
                    PageSize = 50,
                    PageSizeInOppositeDirection = 50,
                    ReadDirection = ReadDirection.Next,
                    StartFromPage = StartingPage.Specific,
                    StartingBookmark = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADMAMQA1ADUAAAAAAA==")
                },
                state: ref mf_1_s_c_8
                );

            //==============================================
            //  GetPage

            mf_1_s_c_9.CurrentRecord = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADMAMQA1ADUAAAAAAA==");
            mf_1_s_c_9.InsertLowerBoundBookmark = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADMAMQA1ADMAAAAAAA==");
            mf_1_s_c_9.InsertUpperBoundBookmark = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADMAMQA1ADUAAAAAAA==");
            mf_1_s_c_9.FlushDataCache = true;
            mf_1_s_c_9.PersonalizationId = "374f138d-58a0-4b24-b6f9-da451371b033";
            mf_1_s_c_9.RecordState = NavRecordOperationTypes.InDatabase;
            mf_1_s_c_9.RenamingMode = RenamingMode.SingleKeyServerSide;
            mf_1_s_c_9.SearchFilter = new NavFilterGroup()
            {
                FilterGroupNo = -1,
                Filters = new NavFilter[0]
            };

            service.GetPage(
                pageRequestDefinition: new PageRequestDefinition()
                {
                    ExcludeStartingRecord = true,
                    IncludeMoreDataInformation = true,
                    IncludeNonRowData = false,
                    IsSubFormUpdateRequest = false,
                    LookupFieldIds = null,
                    LookupFieldValues = null,
                    NormalFields = null,
                    PageSize = 50,
                    PageSizeInOppositeDirection = 0,
                    ReadDirection = ReadDirection.Previous,
                    StartFromPage = StartingPage.Specific,
                    StartingBookmark = Convert.FromBase64String("JgAAAACLAgAAAAJ7/x8EHgQQBBIELQAyADEALQAwADMAMAA5ADIAAAAAAA==")
                },
                state: ref mf_1_s_c_9
                );





            ////*************************************************
            ////  НАЖАТЬ НА КНОПКУ УЧЕСТЬ ОТЧЕТ
            ////*************************************************

            //==============================================
            //  ActionField
            //NavRecordState af_s_1 = nofa_1_mf.State;
            NavDataSet af_nds_1 = nofa_1_mf.DataSet;

            //извлекаем из структуры NAV отражением и проверяем что внурри до перезаписи
            var recdatasetref_10 = af_nds_1.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var recdataset_10 = recdatasetref_10.GetValue(af_nds_1);
            byte[] databyte_10 = (byte[])recdataset_10.GetType().GetProperty("Data").GetValue(recdataset_10);

            string datastr_original_10 = System.Text.Encoding.UTF8.GetString((byte[])databyte_10);

            //************************************
            //       DATA FIELD CORRELATION
            //записываем в структуру NAV отражением
            string DataSet_str_af_1 = "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";
            byte[] DataSet_af_1 = Convert.FromBase64String(DataSet_str_af_1);

            string datastr_original_11 = System.Text.Encoding.UTF8.GetString((byte[])DataSet_af_1);


            for (int i = 0; i < DataSet_af_1.Length; i++)
            {
                if (DataSet_af_1.Skip(i).Take(poav_pattern.Length).SequenceEqual(poav_pattern))
                {
                    correlate_data_field(i + 8, DataSet_af_1, POAV_NUMBER);
                }

                if (DataSet_af_1.Skip(i).Take(poav_pattern2.Length).SequenceEqual(poav_pattern2))
                {
                    correlate_data_field_id2(i + 21, DataSet_af_1, POAV_NUMBER);
                }

                if (DataSet_af_1.Skip(i).Take(poav_pattern3.Length).SequenceEqual(poav_pattern3))
                {
                    correlate_data_field_id2(i + 21, DataSet_af_1, POAV_NUMBER);
                }
            }

            NavDataSet NavDataSet_10 = new NavDataSet { DataSetName = "Purchase Header" };

            var datasetdata_10 = NavDataSet_10.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(NavDataSet_10);
            datasetdata_10.GetType().GetProperty("Data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(datasetdata_10, DataSet_af_1);
            NavDataSet_10.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(NavDataSet_10, datasetdata_10);

            af_nds_1 = NavDataSet_10;   //запись правильного поля Data, автоматически генерируется неправильно

            //извлекаем из структуры NAV отражением и проверяем что записалось
            var recdatasetref_11 = af_nds_1.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var recdataset_11 = recdatasetref_11.GetValue(af_nds_1);
            byte[] databyte_11 = (byte[])recdataset_11.GetType().GetProperty("Data").GetValue(recdataset_11);

            string datastr_original_12 = System.Text.Encoding.UTF8.GetString((byte[])databyte_11);
            //************************************


            mf_1_s_c_7.PersonalizationId = "374f138d-58a0-4b24-b6f9-da451371b033";
            mf_1_s_c_7.PageCaption = "";
            mf_1_s_c_7.RecordState = NavRecordOperationTypes.Navigation;
            mf_1_s_c_7.RenamingMode = RenamingMode.SingleKeyServerSide;

            service.ActionField(state: ref mf_1_s_c_7, recDataSet: af_nds_1, 52, 52);



            //Console.WriteLine("POAV={0}",poav_filter);
            //for (int bbb = 0; bbb < 200; bbb++)
            //{
            //    for (int aaa = 0; aaa < 200; aaa++)
            //    {
            //        try
            //        {

            //            service.ActionField(state: ref mf_1_s_c_7, recDataSet: af_nds_1, aaa, bbb);

            //            Console.WriteLine("Success = {0}", aaa);
            //        }
            //        catch (Microsoft.Dynamics.Nav.Types.NavTestFieldException)
            //        {
            //            Console.WriteLine("NavTestFieldException = {0}, {1}", aaa, bbb);
            //        }
            //        catch (Microsoft.Dynamics.Nav.Types.Exceptions.NavCSideException) {
            //            Console.WriteLine("NavCSideException = {0}, {1}", aaa, bbb);
            //        }
            //        catch
            //        {

            //        }
            //    }
            //}

            //if (i == 5557)
            //{
            //    int bbb = 1;
            //}
            ////*************************************************
            ////  ПРОЦЕСС УЧЕТА
            ////*************************************************

            //==============================================
            //  EndClientCall
            object[] ecc_1 = new object[1]; ecc_1[0] = true;

            //service.EndClientCall(ecc_1);

            ////==============================================
            ////  OpenDialogResponse


            //==============================================
            //  UpdateDialogResponse


            //==============================================
            //  CloseDialogResponse            

            //==============================================
            //  GetPage

            //service.GetPage(
            //    pageRequestDefinition: new PageRequestDefinition()
            //    {
            //        IncludeMoreDataInformation = true,
            //        IncludeNonRowData = true,
            //        PageSize = 50,
            //        PageSizeInOppositeDirection = 50,
            //        ReadDirection = ReadDirection.Next,
            //        StartFromPage = StartingPage.Specific,
            //        StartingBookmark = Convert.FromBase64String(gp_sb_1)
            //    },
            //    state: ref gp_mf_1
            //    );


            ////*************************************************
            ////  ЗАКРЫТИЕ ПРИЛОЖЕНИЯ
            ////*************************************************

            ////==============================================
            ////  CloseForm
            //{
            //    NavCloseFormArguments ncfa_1_mf = new NavCloseFormArguments()
            //    {
            //        ExitAction = FormResult.OK,
            //        DataSet = nofa_1_mf.DataSet,
            //        State = nofa_1_mf.State
            //    };

            //    NavCloseFormArguments[] ncfa_1_sf = null;

            //    //************************************
            //    //       DATA FIELD CORRELATION
            //    string DataSet_str_cf_3 = "Ck5ld0RhdGFTZXR/AAAAAQAAAAEwCQQAALUAAAAIYm9va21hcmsTAAAAAAEwCgAAAAABMQgAAAAAATMRAAAAAAEyEQAAAAABNBEAAAAAATURAAAAAAE2EQAAAAABNxEAAAAAATgRAAAAAAE5EQAAAAACMTARAAAAAAIxMREAAAAAAjEyEQAAAAACMTMRAAAAAAIxNBEAAAAAAjE1EQAAAAACMTYRAAAAAAIxNxEAAAAAAjE4EQAAAAACMTkPAAAAAAIyMA8AAAAAAjIxDwAAAAACMjIRAAAAAAIyMxEAAAAAAjI0DwAAAAACMjUOAAAAAAIyNg8AAAAAAjI3EQAAAAACMjgRAAAAAAIyOREAAAAAAjMwEQAAAAACMzERAAAAAAIzMhEAAAAAAjMzDgAAAAACMzUCAAAAAAIzNxEAAAAAAjQxEQAAAAACNDMRAAAAAAI0NREAAAAAAjQ3CAAAAAACNTERAAAAAAI1MggAAAAAAjUzEQAAAAACNTURAAAAAAI1NwIAAAAAAjU4AgAAAAACNTkCAAAAAAI2MhEAAAAAAjYzEQAAAAACNjQRAAAAAAI2NREAAAAAAjY2EQAAAAACNjcRAAAAAAI2OBEAAAAAAjY5EQAAAAACNzARAAAAAAI3MhEAAAAAAjczEQAAAAACNzQRAAAAAAI3NhEAAAAAAjc3EQAAAAACNzgRAAAAAAI3OREAAAAAAjgwEQAAAAACODERAAAAAAI4MhEAAAAAAjgzEQAAAAACODQRAAAAAAI4NREAAAAAAjg2EQAAAAACODcRAAAAAAI4OBEAAAAAAjg5EQAAAAACOTARAAAAAAI5MREAAAAAAjkyEQAAAAACOTMRAAAAAAI5NAgAAAAAAjk1EQAAAAACOTcRAAAAAAI5OAIAAAAAAjk5DwAAAAADMTAxEQAAAAADMTAyEQAAAAADMTA0EQAAAAADMTA3EQAAAAADMTA4EQAAAAADMTA5EQAAAAADMTE0EQAAAAADMTE1AgAAAAADMTE2EQAAAAADMTE4EQAAAAADMTE5DgAAAAADMTIwCAAAAAADMTIxCAAAAAADMTIyDgAAAAADMTIzAgAAAAADMTI0CAAAAAADMTI1EQAAAAADMTI2EQAAAAADMTI5CAAAAAADMTMwEQAAAAADMTMxEQAAAAADMTMyEQAAAAADMTMzEQAAAAADMTM0DgAAAAADMTM1EQAAAAADMTM2AgAAAAADMTM3DwAAAAADMTM4EQAAAAADMTM5EQAAAAADMTQyDwAAAAADMTQzEQAAAAADMTQ0DgAAAAADMTUxEQAAAAADMTYwCAAAAAADMTYxEgAAAAADMTY1CAAAAAADMTcwEQAAAAADMTcxEQAAAAADNDgwCAAAAAAENTA0OAgAAAAABDUwNTARAAAAAAQ1MDUyEQAAAAAENTA1MxEAAAAABDU3MDARAAAAAAQ1NzUzCAAAAAAENTc5MA8AAAAABDU3OTEPAAAAAAQ1NzkyEQAAAAAENTc5MxEAAAAABDU4MDARAAAAAAQ1ODAxEQAAAAAENTgwMhEAAAAABDU4MDMCAAAAAAQ1ODA0EQAAAAAEODAwMBIAAAAABDkwMDARAAAAAAUxMjQwMAIAAAAABTEyNDAxEQAAAAAFMTI0MDIRAAAAAAUxMjQwMwgAAAAABTEyNDA0AgAAAAAFMTI0MzcIAAAAAAUxMjQzOAgAAAAABTEyNDQwAgAAAAAFMTI0NDEIAAAAAAUxMjQ0MhEAAAAABTEyNDQzCAAAAAAFMTI0NDQRAAAAAAUxMjQ0NREAAAAABTEyNDQ2CAAAAAAFMTI0NDcRAAAAAAUxMjQ3MBEAAAAABTEyNDcxDwAAAAAFMTI0NzIPAAAAAAUxMjQ3Mw8AAAAABTEyNDc0EQAAAAAFMTI0ODURAAAAAAUxMjQ4NgIAAAAABTEyNDkwEQAAAAAFMTI0OTERAAAAAAUxMjQ5OAIAAAAABTEyNDk5DwAAAAAENTc1NBEAAAAABDU3OTYPAAAAAAI0NgIAAAAAAjU2AgAAAAACNjAOAAAAAAI2MQ4AAAAAAzMwMA4AAAAAAzMwMQ4AAAAABDEzMDUOAAAAAAQ1MDQzCAAAAAAENTc1MQIAAAAABDU3NTICAAAAAAQ5MDAxCAAAAAAFMTI0ODAOAAAAABhPcGVuQXBwcm92YWxFbnRyaWVzRXhpc3QCAAAAABhDb250cm9sMTEwMjYwMTAwOV9Gb3JtYXQRAAAAAAEAAAACKwAAACYAAAAAiwIAAAACe/8fBB4EEAQSBC0AMgAxAC0AMAAwADAAMgA2AAAAAAACQJkNAAAAAAACAgAAAAIR0J/QntCQ0JItMjEtMDAwMjYCAAIAAgACAAIAAgACAAIAAgACAAIaQ1JPTlVTINCg0L7RgdGB0LjRjyDQl9CQ0J4CAAIL0KDQuNC90LMsIDUCAAIM0JzQvtGB0LrQstCwAgACAEAAU43B2AgCAAAAAgBAAFONwdgIAgAAAAIAQABTjcHYCAIAAAACGUludm9pY2Ug0J/QntCQ0JItMjEtMDAwMjYCAAIAAAAAAAAAAAIAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAIAAAACAAIAAgACAAIAAgACAAAAAAAAAAAAAAAAAAAAAAIAAgACAAIAAgACAAAAAAIAAgAAAAACAAIAAgACAAIAAhHQn9Ce0JDQki0yMS0wMDAyNgIR0J/QntCQ0JItMjEtMDAwMjYCAAIAAgACAAIR0J/QntCQ0JItMjEtMDAwMjYCAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgYxMDMwNTQCAAICUlUCAAAAAAIAAgACAAIAQABTjcHYCAIAAAACAAIAAgACCdCf0J7Qmi0yMAIJ0J/QntCaLTIwAgnQn9Ce0JotMTUCAAIAAgACAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAACAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgACAAAAAAIAAgACAAAAAAIAAgACAAIAAgAAAAAAAAAAAAAAAAAAAAACAAIBAgAAAAAAAAAAAgAAAAIAAgACAAAAAAAAAAACAAAAAgACAAAAAAAAAAAAAAAAAAAAAAIAAgAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAgACAAIAAAAAAgEAAAACAAIAAgACAAIAAAAAAgAAAAAAAAAAAgAAAAIAAAAAAAAAAAIAAAACAAIAAgACAAIAAgACAAJBH4v7Xzg+TJf32lH6uKMsAgACAQIAAgACAAAAAAIAAgAAAAACAAAAAAIAAgAAAAACAAIAAAAAAgACAAIAAAAAAgACAAIAAAAAAAAAAAIAAAACAAAAAAAAAAACAAAAAgAAAAAAAAAAAgAAAAIAAgACAAIAAgACAAIAAAAAAAAAAAIAAAAAAAIAAgACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAACAAIAAgAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAgsjLCMjMC4jIyMjIw==";
            //    byte[] DataSet_cf_3 = Convert.FromBase64String(DataSet_str_cf_3);

            //    NavDataSet NavDataSet_cf_3 = new NavDataSet { DataSetName = "Purchase Header" };

            //    var datasetdata_cf_3 = NavDataSet_cf_3.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(NavDataSet_cf_3);
            //    datasetdata_cf_3.GetType().GetProperty("Data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(datasetdata_cf_3, DataSet_cf_3);
            //    NavDataSet_cf_3.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(NavDataSet_cf_3, datasetdata_cf_3);

            //    ncfa_1_mf.DataSet = NavDataSet_cf_3;
            //    //************************************

            //    service.CloseForm(
            //        ref ncfa_1_mf,
            //        ref ncfa_1_sf,
            //        force: false);
            //}

            ////==============================================
            ////  OpenCompany, CloseConnection
            //navWinFormsClientSession.OnExit();




            ////Console.ReadLine();

            return 0;
        }


        static byte[] returnDataByte(NavDataSet DataSet)
        {
            var DataSetRef = DataSet.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var recdataset = DataSetRef.GetValue(DataSet);
            byte[] databyte = (byte[])recdataset.GetType().GetProperty("Data").GetValue(recdataset);
            return databyte;
        }



        static void correlate_data_field(int i, byte[] dest, byte[] src)
        {

            byte[] databyte = dest.Skip(i /*i+8 poav*/).ToArray().Take(9).ToArray(); // ПОАВ-20-148
            /* DEST 9 elements
			45 -
			50 2
			48 0
			45 -
			48 0
			48 0
			51 3
			56 8
			57 9*/

            /* SRC 9 elements
			45 -
			50 2
			48 0
			45 -
			48 0
			48 0
			52 4
			52 4
			51 3*/

            dest[i + 0] = src[0];
            dest[i + 1] = src[1];
            dest[i + 2] = src[2];
            dest[i + 3] = src[3];
            dest[i + 4] = src[4];
            dest[i + 5] = src[5];
            dest[i + 6] = src[6];
            dest[i + 7] = src[7];
            dest[i + 8] = src[8];

            /*17 elements
			208 _p
			159 P
			208 _o
			158 O
			208 _a
			144 A
			208 _v
			146 V
			45 -
			50 2
			48 0
			45 -
			48 0
			48 0
			51 3
			56 8
			57 9*/
        }

        static void correlate_data_field_id2(int i, byte[] dest, byte[] src)
        {
            byte[] databyte = dest.Skip(i /*i+21 &{*/).ToArray().Take(22).ToArray(); // ПОАВ-20-148
            /* DEST 22 elements
			45 -
			0
			50 2
			0
			48 0
			0
			45 -
			0
			48 0
			0
			48 0
			0
			51 3
			0
			56 8
			0
			57 9
			0
			0
			0
			0
			0*/

            /* SRC 9 elements
			45 -
			50 2
			48 0
			45 -
			48 0
			48 0
			52 4
			52 4
			51 3*/

            dest[i + 0] = src[0];
            dest[i + 2] = src[1];
            dest[i + 4] = src[2];
            dest[i + 6] = src[3];
            dest[i + 8] = src[4];
            dest[i + 10] = src[5];
            dest[i + 12] = src[6];
            dest[i + 14] = src[7];
            dest[i + 16] = src[8];
        }

        static void correlate_poav_number(byte[] dest, byte[] src)
        {
            dest[21] = src[0];
            dest[23] = src[1];
            dest[25] = src[2];
            dest[27] = src[3];
            dest[29] = src[4];
            dest[31] = src[5];
            dest[33] = src[6];
            dest[35] = src[7];
            dest[37] = src[8];

            /*
			
			"&\0\0\0\0�\u0002\0\0\0\u0002{�\u001f\u0004\u001e\u0004\u0010\u0004\u0012\u0004-\02\00\0-\00\00\03\08\09\0\0\0\0\0"

			38 & 
			0
			0
			0
			0
			139 {
			2
			0
			0
			0
			2
			123
			255
			31
			4
			30
			4
			16
			4
			18
			4
			45 -
			0
			50 2
			0
			48 0
			0
			45 -
			0
			48 0
			0
			48 0
			0
			50 2
			0
			49 1
			0
			48 0
			0
			0
			0
			0
			0

			"\00\07\0\0\0\0\0\u0002\u0002\0\0\0\u0002\0\u0002\u0011ПОАВ-20-00407\u0002\0\u0002\0\u0002"

			byte[] databyte2 = databyte1.Skip(1698).ToArray().Take(17).ToArray();

			208 П
			169 П
			208 О
			158 О
			208 А
			144 А
			208 В
			146 В
			45 -
			50 2
			48 0
			45 -
			48 0
			48 0
			52 4
			49 1
			57 9

			"ПОАВ-20-00419"
			
			*/
        }




    }


}
