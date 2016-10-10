using Newtonsoft.Json.Linq;

namespace PBA_Application
{
    internal class CallDetailItem
    {
        internal string _callDate, _callTime, _phoneNo, _duration, _comments;
        internal float _callCost;
        internal string _freeCall, _roamingCall, _smsCall, _stdCall, _callDirection;
        internal int _pulse;

        public CallDetailItem()
        {
            _callDate = _callTime = _phoneNo = _duration = _comments = "";
            _freeCall = _roamingCall = _smsCall = _stdCall = _callDirection = "";
            _callCost = 0;
            _pulse = 0;
        }

        public CallDetailItem(string call_detail_string)
        {
            _callDate = _callTime = _phoneNo = _duration = _comments = "";
            _freeCall = _roamingCall = _smsCall = _stdCall = _callDirection = "";
            _callCost = 0;
            _pulse = 0;

            try
            {
                JObject call_detail = JObject.Parse(call_detail_string);

                _callDate = call_detail.GetValue("callDate").ToString();
                _callTime = call_detail.GetValue("callTime").ToString();
                _phoneNo = call_detail.GetValue("phoneNumber").ToString();
                _duration = call_detail.GetValue("duration").ToString();
                _callCost = (float)call_detail.GetValue("cost");
                _comments = call_detail.GetValue("comments").ToString();
                _callDirection = call_detail.GetValue("callDirection").ToString();

                _freeCall = call_detail.GetValue("freeCall").ToString();
                _roamingCall = call_detail.GetValue("roamingCall").ToString();
                _smsCall = call_detail.GetValue("smsCall").ToString();
                _stdCall = call_detail.GetValue("stdCall").ToString();
                _pulse = (int)call_detail.GetValue("pulse");

            }
            catch (System.Exception e)
            {
                // Can't do anything
            }
        }
    }
}