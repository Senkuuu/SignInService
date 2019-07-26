using Quartz;
using System;
using log4net;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SignIn
{
    /// <summary>
    /// 测试服后台定时任务
    /// </summary>
    [DisallowConcurrentExecution]
	[PersistJobDataAfterExecution]
	public class TimingJob : IJob
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(TimingJob));

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var client = new RestClient("https://web.iusung.com");
                var loginRequest = new RestRequest("api/App/User/Login", Method.POST);
                loginRequest.AddParameter("application/json",
                       JsonConvert.SerializeObject(new LoginReq()
                       {
                           MobilePhone = "13117514506",
                           Password = "da64d806bd0719977680738625814811",
                           Version = "2.07"
                       }),
                        ParameterType.RequestBody);
                var loginResponse = client.Execute(loginRequest);
                _logger.Debug("api/App/User/Login  " + loginResponse.Content);
                if (loginResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var orderRequest = new RestRequest("api/App/EpSign/GetMySignOrder", Method.GET);
                    foreach (var item in loginResponse.Cookies)
                    {
                        orderRequest.AddCookie(item.Name, item.Value);
                    }
                    var orderResponse = client.Execute(orderRequest);
                    _logger.Debug("api/App/User/Login  " + orderResponse.Content);
                    if (orderResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var order = JsonConvert.DeserializeObject<OrderResp>(orderResponse.Content);
                        if (order.Items != null && order.Items.Count > 0)
                        {
                            var orderIds = new List<string>();
                            foreach (var item in order.Items)
                            {
                                orderIds.Add(item.OrderId);
                            }
                            var signRequest = new RestRequest("api/App/EpSign/Sign", Method.POST);
                            foreach (var item in loginResponse.Cookies)
                            {
                                signRequest.AddCookie(item.Name, item.Value);
                            }
                            signRequest.AddParameter("application/json",
                                 JsonConvert.SerializeObject(new SignReq()
                                 {
                                     OrderIds = orderIds,
                                     Coord = new Coord(28.21347823, 112.97935279)
                                 }),
                                ParameterType.RequestBody);
                            var signResponse = client.Execute(signRequest);
                            _logger.Debug("api/App/User/Login  " + signResponse.Content);
                            if (signResponse.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                _logger.Debug(DateTime.Now + "签到成功");
                            }
                        }
                    }
                }
                await Task.Delay(1);
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }
    }

    public class LoginReq
    {
        public string MobilePhone { get; set; }
        public string Password { get; set; }
        public string Version { get; set; }
    }

    public class OrderResp
    {
        [JsonProperty("items")]
        public IList<SignOrder> Items { get; set; }

        //
        // 摘要:
        //     返回: 0=成功 1=失败,401=未登录，404=未找到，500=系统内部错误，其它=错误码
        [JsonProperty("error")]
        public int Code { get; set; }
        //
        // 摘要:
        //     失败原因说明
        [JsonProperty("msg")]
        public string Message { get; set; }
        //
        // 摘要:
        //     数据总条数
        [JsonProperty("total")]
        public long Total { get; set; }
        //
        // 摘要:
        //     数据总页数
        [JsonProperty("pages")]
        public long Pages { get; set; }

    }

    /// <summary>
    /// 点名订单详情
    /// </summary>
    public class SignOrder
    {
        /// <summary>
        /// 企业Id
        /// </summary>
        public string OrgId { get; set; }
        /// <summary>
        /// 企业名称
        /// </summary>
        public string OrgName { get; set; }
        /// <summary>
        /// 是否签到有奖
        /// </summary>
        public bool IsSignBonus { get; set; }
        /// <summary>
        /// 签到奖励积分
        /// </summary>
        public double SignGivePoint { get; set; }
        /// <summary>
        /// 是否签到定位
        /// </summary>
        public bool IsSignLoc { get; set; }
        /// <summary>
        /// 点名订单ID
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// 被点名的用户ID
        /// </summary>
        internal string UserId { get; set; }

        /// <summary>
        /// 当前时间（APP端需采用服务器时间，用来计算剩余签到时间，不能用机器时间，否则可以作弊）
        /// </summary>
        public DateTime ThisTime { get; set; }
        /// <summary>
        /// 点名时间
        /// </summary>
        public DateTime SignTime { get; set; }
        /// <summary>
        /// 弹性时间（点名时间+弹性时间（分钟）>当前时间，可提交确认点名）
        /// </summary>
        public int ElasticityTime { get; set; }
        /// <summary>
        /// 该订单是否需要收集地理位置信息
        /// </summary>
        [JsonIgnore]
        public bool IsGeo { get { return IsSignLoc; } }
        /// <summary>
        /// 该订单是否已经签到，如果为false，则可以提交进行确认点名，如果为true则表示该订单已经操作签到了，不用再签到了
        /// </summary>
        public bool IsCheck { get; set; }

        /// <summary>
        /// 地理位置中心点是否启用
        /// </summary>
        public bool IsPointSet { get; set; }

        /// <summary>
        /// 地理位置中心点
        /// </summary>
        public Coord Coord { get; set; }
        /// <summary>
        /// 中心点位置文本
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 点名中心点的距离，单位【米】
        /// </summary>
        public int? Distance { get; set; }
    }

    public class Coord
    {
        /// <summary>
        /// 
        /// </summary>
        public Coord() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        public Coord(double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
        }
        /// <summary>
        /// 纬度坐标(小的那个数字)
        /// 39.888411
        /// </summary>
        [JsonProperty("lat")]
        public double Lat { get; set; }
        /// <summary>
        /// 经度坐标（大的那个数字）
        /// 116.333097
        /// </summary>
        [JsonProperty("lng")]
        public double Lng { get; set; }
    }

    public class SignReq
    {
        /// <summary>
		/// 点名订单ID集合（多个企业一起签到处理）
		/// </summary>
        [JsonProperty("orderIds")]
		public IList<string> OrderIds { get; set; }
        /// <summary>
        /// 位置信息 - 如果订单要求收集位置信息则上传该对象
        /// 如果不需要收集或者无法获取位置信息 请传null 或者坐标值赋0
        /// </summary>
        [JsonProperty("coord")]
        public Coord Coord { get; set; }
    }
}


