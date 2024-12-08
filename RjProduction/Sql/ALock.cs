
namespace RjProduction.Sql
{
    public class ALock
    {
        private Timer LockTimer;
        private readonly ISqlProfile _sqlProfile;
        private readonly SqlParam LockObj;
        private bool _wait = false;
        private bool _unlock = false;

        public ALock(ISqlProfile sqlProfile, SqlParam obj)
        {
            LockObj = obj;
            _sqlProfile = sqlProfile;
            LockTimer = new Timer(new TimerCallback(CallBack), null, 0, 30000);
        }

        private void CallBack(object? obj)
        {
            if (LockObj.ID != -1) return;
            // Первых вход в метод открывает прозвон возможности сделать запись 
            if (_wait == false)
            {
                LockTimer = new Timer(new TimerCallback(CallBack), null, 0, 150);
                _wait = true;
            }
            // если коннект свободен то будет выполнена запись
            if (_sqlProfile.ConnectIs == true)
            {
                string sql;
                if (_unlock) sql = $"/* unlock */DELETE FROM Premision WHERE tabel_name='{LockObj.TabelName}' and index_id={LockObj.ID} and user='{MDL.UserName}' ";
                else sql = $"/* CallBack update */UPDATE Premision SET timeout = time('now', '5 minutes') WHERE  tabel_name= '{LockObj.TabelName}' and index_id = {LockObj.ID} and user ='{MDL.UserName}'";

                _sqlProfile.Conection();
                try
                {
                    _sqlProfile.SqlCommand(sql);
                }
                finally
                {
                    if (_unlock)
                    {
                        // объект успешно разблокирован
                        LockTimer.Dispose();
                        _sqlProfile.Disconnect();
                        _wait = false;
                    }
                    else
                    {
                        // объект успешно заблокирован
                        _sqlProfile.Disconnect();
                        LockTimer = new Timer(new TimerCallback(CallBack), null, 0, 30000);
                        _wait = false;
                    }
                }
            }
        }

        public void Unlock()
        {
            _unlock = true;
            LockTimer = new Timer(new TimerCallback(CallBack), null, 0, 150);
            _wait = true;
        }

        ~ALock()
        {
            // Немедленно разблокирует запись иначе она останеться зависшей в течение таймера
            if (_sqlProfile.ConnectIs == false) return;
            string sql = $"/* unlock */DELETE FROM Premision WHERE tabel_name='{LockObj.TabelName}' and index_id={LockObj.ID} and user='{MDL.UserName}' ";
            _sqlProfile.Conection();
            try
            {
                _sqlProfile.SqlCommand(sql);
            }
            finally
            {
                // объект успешно разблокирован
                LockTimer.Dispose();
                _sqlProfile.Disconnect();
            }
        }

    }
}
