using cszmcaux;
using ImTools;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.DryIoc;
using Prism.Mvvm;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using ZMotionWindow.Extensions;
using ZMotionWindow.Models;
using ZMotionWindow.ZMotion;
using ZMotionWindow.ZMotion.enums;
using static cszmcaux.Zmcaux;

namespace ZMotionWindow.ViewModels
{
    internal class MainViewModel : BindableBase
    {
        public DelegateCommand ScanIPCommand { get; set; }
        public DelegateCommand ConnectOrCloseCommand { get; set; }
        public DelegateCommand InitControlCommand { get; set; }
        public DelegateCommand OpenOscilloscopeCommand { get; set; }
        public DelegateCommand PositiveMoveCommand { get; set; }
        public DelegateCommand NegativeMoveCommand { get; set; }
        public DelegateCommand InchMoveCommand { get; set; }
        public DelegateCommand ResetPositionCommand { get; set; }
        public DelegateCommand StopMovingCommand { get; set; }
        public DelegateCommand ReturnZeroMotionCommand { get; set; }
        public DelegateCommand<OutStatusPanel> ChangeOutStatusCommand { get; set; }

        private Timer _slowTimer;
        private IntPtr _handle;
        private int _axis;//轴号
        private int _fwdIn;//正向软限位IO口
        private int _revIn;//负向软限位IO口
        private int _datumIn;//设置原点位映射的IO口
        private ZmotionStatus _zmotionStatus;

        private SnackbarMessageQueue _messageQueue;

        public SnackbarMessageQueue MessageQueue
        {
            get { return _messageQueue; }
            set { _messageQueue = value; }
        }


        private ObservableCollection<string> _ipAddressList;

        public ObservableCollection<string> IpAddressList
        {
            get { return _ipAddressList; }
            set { _ipAddressList = value; }
        }

        private ObservableCollection<InStatusPanel> _inStatusPanels;

        public ObservableCollection<InStatusPanel> InStatusPanels
        {
            get { return _inStatusPanels; }
            set { _inStatusPanels = value; }
        }

        private ObservableCollection<OutStatusPanel> _outStatusPanels;

        public ObservableCollection<OutStatusPanel> OutStatusPanels
        {
            get { return _outStatusPanels; }
            set { _outStatusPanels = value; }
        }

        private string _selectIP;

        public string SelectIP
        {
            get { return _selectIP; }
            set
            {
                _selectIP = value;
                RaisePropertyChanged();
            }
        }

        private bool _isConnect;

        public bool IsConnect
        {
            get { return _isConnect; }
            set
            {
                _isConnect = value;
                RaisePropertyChanged();
            }
        }

        private bool _isConnectEnable;

        public bool IsConnectEnable
        {
            get { return _isConnectEnable; }
            set
            {
                _isConnectEnable = value;
                RaisePropertyChanged();
            }
        }

        private string _startingSpeed;
        /// <summary>
        /// 轴起始速度
        /// </summary>
        public string StartingSpeed
        {
            get { return _startingSpeed; }
            set
            {
                _startingSpeed = value;
                RaisePropertyChanged();
            }
        }

        private string _runningSpeed;
        /// <summary>
        /// 轴速度
        /// </summary>
        public string RunningSpeed
        {
            get { return _runningSpeed; }
            set
            {
                _runningSpeed = value;
                RaisePropertyChanged();
            }
        }

        private string _acceleration;
        /// <summary>
        /// 轴加速度
        /// </summary>
        public string Acceleration
        {
            get { return _acceleration; }
            set
            {
                _acceleration = value;
                RaisePropertyChanged();
            }
        }

        private string _deceleration;
        /// <summary>
        /// 轴减速度
        /// </summary>
        public string Deceleration
        {
            get { return _deceleration; }
            set
            {
                _deceleration = value;
                RaisePropertyChanged();
            }
        }

        private string _units;
        /// <summary>
        /// 脉冲当量
        /// </summary>
        public string Units
        {
            get { return _units; }
            set
            {
                _units = value;
                RaisePropertyChanged();
            }
        }

        private string _sramp;
        /// <summary>
        /// 轴的S曲线时间
        /// </summary>
        public string Sramp
        {
            get { return _sramp; }
            set
            {
                _sramp = value;
                RaisePropertyChanged();
            }
        }

        private string _inchMoveDistance;
        /// <summary>
        /// 寸动距离
        /// </summary>
        public string InchMoveDistance
        {
            get { return _inchMoveDistance; }
            set
            {
                _inchMoveDistance = value;
                RaisePropertyChanged();
            }
        }

        private bool _relativeMove;

        public bool RelativeMove
        {
            get { return _relativeMove; }
            set
            {
                _relativeMove = value;
                RaisePropertyChanged();
            }
        }

        private bool _forwardDirection;

        public bool ForwardDirection
        {
            get { return _forwardDirection; }
            set
            {
                _forwardDirection = value;
                RaisePropertyChanged();
            }
        }

        private float _instructDpos;

        public float InstructDpos
        {
            get { return _instructDpos; }
            set
            {
                _instructDpos = value;
                RaisePropertyChanged();
            }
        }

        private float _backDpos;

        public float BackDpos
        {
            get { return _backDpos; }
            set
            {
                _backDpos = value;
                RaisePropertyChanged();
            }
        }

        private string _axisStatusStr;
        /// <summary>
        /// 轴状态
        /// </summary>
        public string AxisStatusStr
        {
            get { return _axisStatusStr; }
            set
            {
                _axisStatusStr = value;
                RaisePropertyChanged();
            }
        }

        public MainViewModel()
        {
            this.ScanIPCommand = new DelegateCommand(ScanIP);
            this.ConnectOrCloseCommand = new DelegateCommand(ConnectOrCloseAsync);
            this.InitControlCommand = new DelegateCommand(InitControl);
            this.PositiveMoveCommand = new DelegateCommand(PositiveMove);
            this.NegativeMoveCommand = new DelegateCommand(NegativeMove);
            this.OpenOscilloscopeCommand = new DelegateCommand(OpenOscilloscope);
            this.InchMoveCommand = new DelegateCommand(InchMove);
            this.ResetPositionCommand = new DelegateCommand(ResetPosition);
            this.StopMovingCommand = new DelegateCommand(StopMoving);
            this.ReturnZeroMotionCommand = new DelegateCommand(ReturnZeroMotion);
            this.ChangeOutStatusCommand = new DelegateCommand<OutStatusPanel>(ChangeOutStatus);
            this._isConnect = false;
            this._isConnectEnable = true;
            this._ipAddressList = new ObservableCollection<string>();
            this._inStatusPanels = new ObservableCollection<InStatusPanel>();
            this._outStatusPanels = new ObservableCollection<OutStatusPanel>();
            this._messageQueue = new SnackbarMessageQueue();
            this.StartingSpeed = "0";
            this.RunningSpeed = "50";
            this.Acceleration = "100";
            this.Deceleration = "100";
            this.Units = "100";
            this.Sramp = "200";
            this.InchMoveDistance = "100";
            this.RelativeMove = true;
            this.ForwardDirection = true;
            _slowTimer = new Timer(100);
            _slowTimer.Elapsed += OnSlowTimedEvent;
            _fwdIn = 4;
            _revIn = 2;
            _datumIn = 0;
            _axis = 2;
        }

        private void ChangeOutStatus(OutStatusPanel curPanel)
        {
            if (ushort.TryParse(curPanel.OutNum, out ushort outNum))
            {
                ZAux_Direct_SetOutMulti(_handle, outNum, outNum, new uint[] { curPanel.OutStatus == 0 ? 1U : 0U });
            }
        }

        private async void ReturnZeroMotion()
        {
            //int res = SetMotionParam();
            //res |= ZAux_Direct_SetCreep(_handle, _axis, 50); //设置回零时反向爬行速度
            //res |= ZAux_Direct_SetHomeWait(_handle, _axis, 1000); //设置回零等待时间
            //res |= ZAux_Direct_Single_Datum(_handle, _axis, 3); //回零，模式 3
            //Task.Run(() =>
            //{
            //    SetMotionParam(0, 200, 200, 400, 100, 0);
            //    ZAux_Direct_Single_Vmove(_handle, _axis, 1);
            //    uint fwdInValue = 0, datumInValue = 0;
            //    while (fwdInValue == 0 && datumInValue == 0)
            //    {
            //        int inMulti;
            //        ZAux_Direct_GetInMulti(_handle, 0, 31, out inMulti);
            //        ZAux_Direct_GetIn(_handle, 4, ref fwdInValue);//获取正向软限位IO口
            //        ZAux_Direct_GetIn(_handle, 6, ref datumInValue);//获取原点位映射的IO口
            //    }

            //    Task.Delay(2000).Wait();
            //    ZAux_Direct_Single_Cancel(_handle, _axis, 3);

            //    SetMotionParam(0, 50, 200, 400, 100, 0);
            //    ZAux_Direct_Single_Vmove(_handle, _axis, -1);
            //    do
            //    {
            //        ZAux_Direct_GetIn(_handle, 6, ref datumInValue);//获取原点位映射的IO口
            //    } while (datumInValue == 0);
            //    ZAux_Direct_Single_Cancel(_handle, _axis, 3);
            //    ZAux_Direct_SetDpos(_handle, _axis, 0);
            //    SetMotionParam();
            //});
            //SetMotionParam(0, 200, 200, 400, 100, 0);
            //ZAux_Direct_Single_Vmove(_handle, _axis, 1);
            //uint fwdInValue = 0, datumInValue = 0;
            //while (fwdInValue == 0 && datumInValue == 0)
            //{
            //    int inMulti;
            //    ZAux_Direct_GetInMulti(_handle, 0, 31, out inMulti);
            //    ZAux_Direct_GetIn(_handle, 4, ref fwdInValue);//获取正向软限位IO口
            //    ZAux_Direct_GetIn(_handle, 6, ref datumInValue);//获取原点位映射的IO口
            //}

            //Task.Delay(2000).Wait();
            //ZAux_Direct_Single_Cancel(_handle, _axis, 3);

            //SetMotionParam(0, 50, 200, 400, 100, 0);
            //ZAux_Direct_Single_Vmove(_handle, _axis, -1);
            //do
            //{
            //    ZAux_Direct_GetIn(_handle, 6, ref datumInValue);//获取原点位映射的IO口
            //} while (datumInValue == 0);
            //ZAux_Direct_Single_Cancel(_handle, _axis, 3);
            //ZAux_Direct_SetDpos(_handle, _axis, 0);
            await CustomZMotion.ReturnZero(_handle, _axis, _zmotionStatus, _fwdIn, _revIn, _datumIn);
            SetMotionParam();
        }

        private void StopMoving()
        {
            int res = ZAux_Direct_Single_Cancel(_handle, _axis, 2);
            ShowMsg(res == 0 ? "停止运动成功！" : "停止运动失败！");
        }

        private void ResetPosition()
        {
            int res = ZAux_Direct_SetDpos(_handle, _axis, 0);
            res = ZAux_Direct_SetMpos(_handle, _axis, 0);
            ShowMsg(res == 0 ? "位置清零成功！" : "位置清零失败！");
        }

        private void InchMove()
        {
            int res = SetMotionParam();
            int moveDirection = ForwardDirection ? 1 : -1;//运动方向
            if (RelativeMove)//运动模式
            {
                res |= ZAux_Direct_Single_MoveAbs(_handle, _axis, Convert.ToSingle(InchMoveDistance) * moveDirection);
            }
            else
            {
                res |= ZAux_Direct_Single_Move(_handle, _axis, Convert.ToSingle(InchMoveDistance) * moveDirection);
            }
            ShowMsg(res == 0 ? "执行寸动成功！" : "执行寸动失败！");
        }

        private void OpenOscilloscope()
        {
            int res = Zmcaux.ZAux_Trigger(_handle);//示波器触发函数
            ShowMsg(res == 0 ? "成功触发示波器！" : "触发示波器失败！");
        }

        private void PositiveMove()
        {
            int res = SetMotionParam();
            res |= ZAux_Direct_Single_Vmove(_handle, _axis, 1);
            ShowMsg(res == 0 ? "开始正向运动" : "开始正向失败！");
        }

        private void NegativeMove()
        {
            int res = SetMotionParam();
            res |= ZAux_Direct_Single_Vmove(_handle, _axis, -1);
            ShowMsg(res == 0 ? "开始反向运动" : "开始反向失败！");
        }

        private int SetMotionParam()
        {
            int res = 0;
            res |= ZAux_Direct_SetLspeed(_handle, _axis, Convert.ToSingle(StartingSpeed)); //设置轴起始速度
            res |= ZAux_Direct_SetSpeed(_handle, _axis, Convert.ToSingle(RunningSpeed)); //设置轴速度
            res |= ZAux_Direct_SetAccel(_handle, _axis, Convert.ToSingle(Acceleration));//设置轴加速度
            res |= ZAux_Direct_SetDecel(_handle, _axis, Convert.ToSingle(Deceleration));//设置轴减速度
            res |= ZAux_Direct_SetUnits(_handle, _axis, Convert.ToSingle(Units)); //设置轴脉冲当量
            res |= ZAux_Direct_SetSramp(_handle, _axis, Convert.ToSingle(Sramp));//设置轴的S曲线时间
            return res;
        }

        private int SetMotionParam(int lSpeed, int speed, int accel, int decel, int _units, int _sramp)
        {
            int res = 0;
            res |= ZAux_Direct_SetLspeed(_handle, _axis, lSpeed); //设置轴起始速度
            res |= ZAux_Direct_SetSpeed(_handle, _axis, speed); //设置轴速度
            res |= ZAux_Direct_SetAccel(_handle, _axis, accel);//设置轴加速度
            res |= ZAux_Direct_SetDecel(_handle, _axis, decel);//设置轴减速度
            res |= ZAux_Direct_SetUnits(_handle, _axis, _units); //设置轴脉冲当量
            res |= ZAux_Direct_SetSramp(_handle, _axis, _sramp);//设置轴的S曲线时间
            return res;
        }

        private void InitControl()
        {
            int res = 0;
            res |= ZAux_Direct_SetAtype(_handle, _axis, 4);//设置轴类型
            res |= ZAux_Direct_SetFwdIn(_handle, _axis, _fwdIn);//设置正向软限位IO口
            res |= ZAux_Direct_SetRevIn(_handle, _axis, _revIn);//设置负向软限位IO口
            res |= ZAux_Direct_SetDatumIn(_handle, _axis, _datumIn);//设置原点位映射的IO口
            res |= ZAux_Direct_SetFsLimit(_handle, _axis, 5000);//设置正向软限位为
            res |= ZAux_Direct_SetRsLimit(_handle, _axis, -5000);//设置负向软限位为
            res |= ZAux_Direct_SetFastDec(_handle, _axis, 4000); //设置快减速度
            res |= ZAux_Direct_SetOp(_handle, 16 + _axis * 2, 1); //打开使能 (OUT16.18.20.22.24.26.28.30)
            if (res == 0)
            {
                InitIOStatusPanels();
                // 启用定时器
                _slowTimer.Enabled = true;
                _zmotionStatus = new ZmotionStatus(_handle); 
                _zmotionStatus.InUpdatedEvent += ZmotionStatus_InUpdatedEvent;
                _zmotionStatus.OutUpdatedEvent += ZmotionStatus_OutUpdatedEvent;
                ShowMsg("初始化成功");
            }
            else
            {
                ShowMsg("初始化失败");
            }
        }

        private void InitIOStatusPanels()
        {
            int len = sizeof(long) * 8;
            long inMulti = CustomZMotion.GetInMulti0_63(_handle);
            for (int inNum = 0; inNum < len; inNum++)
            {
                _inStatusPanels.Add(new InStatusPanel(inMulti & (1L << inNum), inNum));
            }
            long outMulti = CustomZMotion.GetOutMulti0_63(_handle);
            for (int outNum = 0; outNum < len; outNum++)
            {
                _outStatusPanels.Add(new OutStatusPanel(outMulti & (1L << outNum), outNum));
            }
        }

        private async void ConnectOrCloseAsync()
        {
            try
            {
                this.IsConnect = !this.IsConnect;
                this.IsConnectEnable = false;
                if (!this.IsConnect)
                {
                    if (string.IsNullOrEmpty(this._selectIP))
                    {
                        ShowMsg("未选择IP地址！");
                        return;
                    }
                    int ret = await Task.Run(() => Zmcaux.ZAux_OpenEth(this._selectIP, out _handle));
                    if (ret != 0)
                    {
                        ShowMsg("控制器连接失败！");
                        _handle = IntPtr.Zero;
                        return;
                    }
                    this.IsConnect = true;
                    ShowMsg("控制器连接成功！");
                }
                else
                {
                    int ret = await Task.Run(() => Zmcaux.ZAux_Close(_handle)); //关闭连接
                    if (ret != 0)
                    {
                        this.IsConnect = true;
                        ShowMsg("控制器关闭失败！");
                        _handle = IntPtr.Zero;
                        return;
                    }
                    this.IsConnect = false;
                    ShowMsg("控制器连接关闭！");
                    _handle = IntPtr.Zero;
                }
            }
            catch (Exception e)
            {
                ShowMsg(e.Message);
            }
            finally
            {
                this.IsConnectEnable = true;
            }
        }

        private async void ScanIP()
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                int res = await Task.Run(() => ZAux_SearchEthlist(builder, 1024, 1000));
                if (res != 0)
                {
                    ShowMsg("扫描IP地址失败");
                    return;
                }
                this._ipAddressList.Clear();
                this._ipAddressList.Add("127.0.0.1");//模拟控制器地址
                string[] ethlist = builder.ToString().Trim().Split(' ');
                foreach (string eth in ethlist)
                {
                    this._ipAddressList.Add(eth);
                }
                ShowMsg("扫描IP地址成功");
            }
            catch (Exception e)
            {
                ShowMsg(e.Message);
            }
        }

        private void ShowMsg(string msg)
        {
            _messageQueue.Clear();
            _messageQueue.Enqueue(msg);
        }

        private void OnSlowTimedEvent(object sender, ElapsedEventArgs e)
        {
            int res = 0, statusValue = 0;
            float dpos = 0, mpos = 0;
            res |= ZAux_Direct_GetDpos(_handle, _axis, ref dpos);
            res |= ZAux_Direct_GetMpos(_handle, _axis, ref mpos);
            res |= ZAux_Direct_GetAxisStatus(_handle, _axis, ref statusValue); //获取轴状态
            InstructDpos = dpos;
            BackDpos = mpos;
            if (statusValue == 0)
            {
                AxisStatusStr = "正常";
            }
            else
            {
                List<string> statusList = new List<string>();
                foreach (AxisStatus status in Enum.GetValues(typeof(AxisStatus)))
                {
                    if ((statusValue & (int)status) != 0)
                    {
                        statusList.Add(status.GetEnumDescription());
                    }
                }
                AxisStatusStr = string.Join(", ", statusList.ToArray());
            }
        }

        private void ZmotionStatus_InUpdatedEvent(long inMulti)
        {
            int len = sizeof(long) * 8;
            for (int inNum = 0; inNum < len; inNum++)
            {
                _inStatusPanels[inNum].InStatus = inMulti & (1L << inNum);
            }
        }

        private void ZmotionStatus_OutUpdatedEvent(long outMulti)
        {
            int len = sizeof(long) * 8;
            for (int inNum = 0; inNum < len; inNum++)
            {
                _outStatusPanels[inNum].OutStatus = outMulti & (1L << inNum);
            }
        }
    }
}
