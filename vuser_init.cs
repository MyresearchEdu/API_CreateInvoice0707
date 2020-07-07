//---------------------------------------------
//Script Title        :
//Script Description  :
//
//
//Recorder Version    : 
//---------------------------------------------

using System;
using System.Linq;
using Microsoft.Dynamics.Framework.UI;
using Microsoft.Dynamics.Nav.Client.WinClient;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.Types;
using System.Timers;


namespace Script
{
    public partial class VuserClass
    {
        public int vuser_init()
        {


            //*************************************************
            //  ЛОГИН
            //*************************************************

            //==============================================
            //  OpenConnection, SetClientSettings, GetAddInRegistrations
            // Старт клиента            
            lr.start_transaction("1_Login");


            IProductNameProvider productNameProvider = new StaticProductNameProvider("PerformanceLab");
            NavWinFormsClientSessionPreparation navWinFormsClientSessionPreparation = new NavWinFormsClientSessionPreparation(productNameProvider, false);
            navWinFormsClientSession = new NavWinFormsClientSession(navWinFormsClientSessionPreparation);

            UserNamePasswordCredentials unpc = new UserNamePasswordCredentials();
            unpc.UserName = "TESTUSER1";
            unpc.Password = "\\\\\\eRMp2kLgwMLqiiVkMukGZw==";

            lr.end_transaction("1_Login", 1);

            lr.start_transaction("2_Login");


            //***************************************************
            //  Контроль над интерфейсом для ввода пароля
            Timer time1 = new Timer();
            time1.Elapsed += new ElapsedEventHandler(NAV_entering_password);
            time1.Interval = 3000;      // 30 secs
            time1.AutoReset = false;    // Have the timer fire repeated events (true is the default)
            time1.Enabled = true;       // Start the timer

            ///asdasd.Automate();
            //asdasd.CloseModalWindows();
            //***************************************************

            navWinFormsClientSession.Init();

            String personalizationId = navWinFormsClientSession.UISession.SessionPersonalizationId; //a9("SessionPersonalizationId", sw_5, sw_6, (short)7);

            personalizationId1 = "ec29feaf-58ae-4c7a-8c7f-792d5cfd9aa0";
            personalizationId2 = "374f138d-58a0-4b24-b6f9-da451371b033";

            service = navWinFormsClientSession.BuilderSession.Service; //a9("Service", sw_6, sw_7, (short)7);
            metadataService = navWinFormsClientSession.BuilderSession.MetadataService; //a9("MetadataService", sw_7, sw_8, (short)7);

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


            return 0;
        }
    }
}

