//---------------------------------------------
//Script Title        :
//Script Description  :
//
//
//Recorder Version    : 
//---------------------------------------------

using System;
using Microsoft.Dynamics.Nav.Client;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Data;

namespace Script
{
    public partial class VuserClass
    {
        public int vuser_end()
        {

            //*************************************************
            //  �������� ����������
            //*************************************************

            //==============================================
            //  CloseForm
            {
                NavCloseFormArguments ncfa_1_mf = new NavCloseFormArguments()
                {
                    ExitAction = FormResult.OK,
                    DataSet = nofa_1_mf.DataSet,
                    State = nofa_1_mf.State
                };

                NavCloseFormArguments[] ncfa_1_sf = null;

                //************************************
                //       DATA FIELD CORRELATION
                string nds_cf_5_Inv_str = "Ck5ld0RhdGFTZXR/AAAAAQAAAAEwCQQAALUAAAAIYm9va21hcmsTAAAAAAEwCgAAAAABMQgAAAAAATMRAAAAAAEyEQAAAAABNBEAAAAAATURAAAAAAE2EQAAAAABNxEAAAAAATgRAAAAAAE5EQAAAAACMTARAAAAAAIxMREAAAAAAjEyEQAAAAACMTMRAAAAAAIxNBEAAAAAAjE1EQAAAAACMTYRAAAAAAIxNxEAAAAAAjE4EQAAAAACMTkPAAAAAAIyMA8AAAAAAjIxDwAAAAACMjIRAAAAAAIyMxEAAAAAAjI0DwAAAAACMjUOAAAAAAIyNg8AAAAAAjI3EQAAAAACMjgRAAAAAAIyOREAAAAAAjMwEQAAAAACMzERAAAAAAIzMhEAAAAAAjMzDgAAAAACMzUCAAAAAAIzNxEAAAAAAjQxEQAAAAACNDMRAAAAAAI0NREAAAAAAjQ3CAAAAAACNTERAAAAAAI1MggAAAAAAjUzEQAAAAACNTURAAAAAAI1NwIAAAAAAjU4AgAAAAACNTkCAAAAAAI2MhEAAAAAAjYzEQAAAAACNjQRAAAAAAI2NREAAAAAAjY2EQAAAAACNjcRAAAAAAI2OBEAAAAAAjY5EQAAAAACNzARAAAAAAI3MhEAAAAAAjczEQAAAAACNzQRAAAAAAI3NhEAAAAAAjc3EQAAAAACNzgRAAAAAAI3OREAAAAAAjgwEQAAAAACODERAAAAAAI4MhEAAAAAAjgzEQAAAAACODQRAAAAAAI4NREAAAAAAjg2EQAAAAACODcRAAAAAAI4OBEAAAAAAjg5EQAAAAACOTARAAAAAAI5MREAAAAAAjkyEQAAAAACOTMRAAAAAAI5NAgAAAAAAjk1EQAAAAACOTcRAAAAAAI5OAIAAAAAAjk5DwAAAAADMTAxEQAAAAADMTAyEQAAAAADMTA0EQAAAAADMTA3EQAAAAADMTA4EQAAAAADMTA5EQAAAAADMTE0EQAAAAADMTE1AgAAAAADMTE2EQAAAAADMTE4EQAAAAADMTE5DgAAAAADMTIwCAAAAAADMTIxCAAAAAADMTIyDgAAAAADMTIzAgAAAAADMTI0CAAAAAADMTI1EQAAAAADMTI2EQAAAAADMTI5CAAAAAADMTMwEQAAAAADMTMxEQAAAAADMTMyEQAAAAADMTMzEQAAAAADMTM0DgAAAAADMTM1EQAAAAADMTM2AgAAAAADMTM3DwAAAAADMTM4EQAAAAADMTM5EQAAAAADMTQyDwAAAAADMTQzEQAAAAADMTQ0DgAAAAADMTUxEQAAAAADMTYwCAAAAAADMTYxEgAAAAADMTY1CAAAAAADMTcwEQAAAAADMTcxEQAAAAADNDgwCAAAAAAENTA0OAgAAAAABDUwNTARAAAAAAQ1MDUyEQAAAAAENTA1MxEAAAAABDU3MDARAAAAAAQ1NzUzCAAAAAAENTc5MA8AAAAABDU3OTEPAAAAAAQ1NzkyEQAAAAAENTc5MxEAAAAABDU4MDARAAAAAAQ1ODAxEQAAAAAENTgwMhEAAAAABDU4MDMCAAAAAAQ1ODA0EQAAAAAEODAwMBIAAAAABDkwMDARAAAAAAUxMjQwMAIAAAAABTEyNDAxEQAAAAAFMTI0MDIRAAAAAAUxMjQwMwgAAAAABTEyNDA0AgAAAAAFMTI0MzcIAAAAAAUxMjQzOAgAAAAABTEyNDQwAgAAAAAFMTI0NDEIAAAAAAUxMjQ0MhEAAAAABTEyNDQzCAAAAAAFMTI0NDQRAAAAAAUxMjQ0NREAAAAABTEyNDQ2CAAAAAAFMTI0NDcRAAAAAAUxMjQ3MBEAAAAABTEyNDcxDwAAAAAFMTI0NzIPAAAAAAUxMjQ3Mw8AAAAABTEyNDc0EQAAAAAFMTI0ODURAAAAAAUxMjQ4NgIAAAAABTEyNDkwEQAAAAAFMTI0OTERAAAAAAUxMjQ5OAIAAAAABTEyNDk5DwAAAAAENTc1NBEAAAAABDU3OTYPAAAAAAI0NgIAAAAAAjU2AgAAAAACNjAOAAAAAAI2MQ4AAAAAAzMwMA4AAAAAAzMwMQ4AAAAABDEzMDUOAAAAAAQ1MDQzCAAAAAAENTc1MQIAAAAABDU3NTICAAAAAAQ5MDAxCAAAAAAFMTI0ODAOAAAAABhPcGVuQXBwcm92YWxFbnRyaWVzRXhpc3QCAAAAABhDb250cm9sMTEwMjYwMTAwOV9Gb3JtYXQRAAAAAAEAAAACKwAAACYAAAAAiwIAAAACe/8fBB4EEAQSBC0AMgAxAC0AMAAwADAAMgA2AAAAAAACQJkNAAAAAAACAgAAAAIR0J/QntCQ0JItMjEtMDAwMjYCAAIAAgACAAIAAgACAAIAAgACAAIaQ1JPTlVTINCg0L7RgdGB0LjRjyDQl9CQ0J4CAAIL0KDQuNC90LMsIDUCAAIM0JzQvtGB0LrQstCwAgACAEAAU43B2AgCAAAAAgBAAFONwdgIAgAAAAIAQABTjcHYCAIAAAACGUludm9pY2Ug0J/QntCQ0JItMjEtMDAwMjYCAAIAAAAAAAAAAAIAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAIAAAACAAIAAgACAAIAAgACAAAAAAAAAAAAAAAAAAAAAAIAAgACAAIAAgACAAAAAAIAAgAAAAACAAIAAgACAAIAAhHQn9Ce0JDQki0yMS0wMDAyNgIR0J/QntCQ0JItMjEtMDAwMjYCAAIAAgACAAIR0J/QntCQ0JItMjEtMDAwMjYCAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgYxMDMwNTQCAAICUlUCAAAAAAIAAgACAAIAQABTjcHYCAIAAAACAAIAAgACCdCf0J7Qmi0yMAIJ0J/QntCaLTIwAgnQn9Ce0JotMTUCAAIAAgACAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAACAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgACAAAAAAIAAgACAAAAAAIAAgACAAIAAgAAAAAAAAAAAAAAAAAAAAACAAIBAgAAAAAAAAAAAgAAAAIAAgACAAAAAAAAAAACAAAAAgACAAAAAAAAAAAAAAAAAAAAAAIAAgAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAgACAAIAAAAAAgEAAAACAAIAAgACAAIAAAAAAgAAAAAAAAAAAgAAAAIAAAAAAAAAAAIAAAACAAIAAgACAAIAAgACAAJBH4v7Xzg+TJf32lH6uKMsAgACAQIAAgACAAAAAAIAAgAAAAACAAAAAAIAAgAAAAACAAIAAAAAAgACAAIAAAAAAgACAAIAAAAAAAAAAAIAAAACAAAAAAAAAAACAAAAAgAAAAAAAAAAAgAAAAIAAgACAAIAAgACAAIAAAAAAAAAAAIAAAAAAAIAAgACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAgAAAAACAAIAAgAAAAACAAAAAAAAAAAAAAAAAAAAAAIAAgsjLCMjMC4jIyMjIw==";
                byte[] nds_cf_5_Inv_byte = Convert.FromBase64String(nds_cf_5_Inv_str);

                NavDataSet nds_cf_5 = new NavDataSet { DataSetName = "Purchase Header" };

                var nds_DS_cf_5 = nds_cf_5.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(nds_cf_5);
                nds_DS_cf_5.GetType().GetProperty("Data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(nds_DS_cf_5, nds_cf_5_Inv_byte);
                nds_cf_5.GetType().GetProperty("DataSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(nds_cf_5, nds_DS_cf_5);

                ncfa_1_mf.DataSet = nds_cf_5;
                //************************************

                service.CloseForm(
                    ref ncfa_1_mf,
                    ref ncfa_1_sf,
                    force: false);
            }

            //==============================================
            //  OpenCompany, CloseConnection
            navWinFormsClientSession.OnExit();

            return 0;
        }
    }
}
