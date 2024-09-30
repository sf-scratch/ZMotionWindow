using cszmcaux;
using ImTools;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.DryIoc;
using Prism.Mvvm;
using System;
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

        private Timer _slowTimer;
        private Timer _fastTimer;
        private IntPtr _handle;
        private int _axis;//轴号
        private int _fwdIn;//正向软限位IO口
        private int _revIn;//负向软限位IO口
        private int _datumIn;//设置原点位映射的IO口
        private IOStatus _ioStatus;

        private SnackbarMessageQueue messageQueue;

        public SnackbarMessageQueue MessageQueue
        {
            get { return messageQueue; }
            set { messageQueue = value; }
        }


        private ObservableCollection<string> ipAddressList;

        public ObservableCollection<string> IpAddressList
        {
            get { return ipAddressList; }
            set { ipAddressList = value; }
        }

        private string selectIP;

        public string SelectIP
        {
            get { return selectIP; }
            set
            {
                selectIP = value;
                RaisePropertyChanged();
            }
        }

        private bool isConnect;

        public bool IsConnect
        {
            get { return isConnect; }
            set
            {
                isConnect = value;
                RaisePropertyChanged();
            }
        }

        private bool isConnectEnable;

        public bool IsConnectEnable
        {
            get { return isConnectEnable; }
            set
            {
                isConnectEnable = value;
                RaisePropertyChanged();
            }
        }

        private string startingSpeed;
        /// <summary>
        /// 轴起始速度
        /// </summary>
        public string StartingSpeed
        {
            get { return startingSpeed; }
            set
            {
                startingSpeed = value;
                RaisePropertyChanged();
            }
        }

        private string runningSpeed;
        /// <summary>
        /// 轴速度
        /// </summary>
        public string RunningSpeed
        {
            get { return runningSpeed; }
            set
            {
                runningSpeed = value;
                RaisePropertyChanged();
            }
        }

        private string acceleration;
        /// <summary>
        /// 轴加速度
        /// </summary>
        public string Acceleration
        {
            get { return acceleration; }
            set
            {
                acceleration = value;
                RaisePropertyChanged();
            }
        }

        private string deceleration;
        /// <summary>
        /// 轴减速度
        /// </summary>
        public string Deceleration
        {
            get { return deceleration; }
            set
            {
                deceleration = value;
                RaisePropertyChanged();
            }
        }

        private string units;
        /// <summary>
        /// 脉冲当量
        /// </summary>
        public string Units
        {
            get { return units; }
            set
            {
                units = value;
                RaisePropertyChanged();
            }
        }

        private string sramp;
        /// <summary>
        /// 轴的S曲线时间
        /// </summary>
        public string Sramp
        {
            get { return sramp; }
            set
            {
                sramp = value;
                RaisePropertyChanged();
            }
        }

        private string inchMoveDistance;
        /// <summary>
        /// 寸动距离
        /// </summary>
        public string InchMoveDistance
        {
            get { return inchMoveDistance; }
            set
            {
                inchMoveDistance = value;
                RaisePropertyChanged();
            }
        }

        private bool relativeMove;

        public bool RelativeMove
        {
            get { return relativeMove; }
            set
            {
                relativeMove = value;
                RaisePropertyChanged();
            }
        }

        private bool forwardDirection;

        public bool ForwardDirection
        {
            get { return forwardDirection; }
            set
            {
                forwardDirection = value;
                RaisePropertyChanged();
            }
        }

        private float instructDpos;

        public float InstructDpos
        {
            get { return instructDpos; }
            set
            {
                instructDpos = value;
                RaisePropertyChanged();
            }
        }

        private float backDpos;

        public float BackDpos
        {
            get { return backDpos; }
            set
            {
                backDpos = value;
                RaisePropertyChanged();
            }
        }

        private string axisStatusStr;
        /// <summary>
        /// 轴状态
        /// </summary>
        public string AxisStatusStr
        {
            get { return axisStatusStr; }
            set
            {
                axisStatusStr = value;
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
            this.isConnect = false;
            this.isConnectEnable = true;
            this.ipAddressList = new ObservableCollection<string>();
            this.messageQueue = new SnackbarMessageQueue();
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
            _fastTimer = new Timer(10);
            _fastTimer.Elapsed += OnFastTimedEvent;
            _fwdIn = 4;
            _revIn = 2;
            _datumIn = 6;
            _ioStatus = new IOStatus();
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
            ReturnZero returnZero = new ReturnZero(_handle, _axis, _ioStatus, _fwdIn, _revIn, _datumIn);
            await returnZero.StartAsync();
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

        private int SetMotionParam(int lSpeed, int speed, int accel, int decel, int units, int sramp)
        {
            int res = 0;
            res |= ZAux_Direct_SetLspeed(_handle, _axis, lSpeed); //设置轴起始速度
            res |= ZAux_Direct_SetSpeed(_handle, _axis, speed); //设置轴速度
            res |= ZAux_Direct_SetAccel(_handle, _axis, accel);//设置轴加速度
            res |= ZAux_Direct_SetDecel(_handle, _axis, decel);//设置轴减速度
            res |= ZAux_Direct_SetUnits(_handle, _axis, units); //设置轴脉冲当量
            res |= ZAux_Direct_SetSramp(_handle, _axis, sramp);//设置轴的S曲线时间
            return res;
        }

        private void InitControl()
        {
            _axis = 3;
            int res = 0;
            res |= ZAux_Direct_SetAtype(_handle, _axis, 1);//设置轴类型为 1
            res |= ZAux_Direct_SetFwdIn(_handle, _axis, _fwdIn);//设置正向软限位IO口
            res |= ZAux_Direct_SetRevIn(_handle, _axis, _revIn);//设置负向软限位IO口
            res |= ZAux_Direct_SetDatumIn(_handle, _axis, _datumIn);//设置原点位映射的IO口
            res |= ZAux_Direct_SetFsLimit(_handle, _axis, 5000);//设置正向软限位为
            res |= ZAux_Direct_SetRsLimit(_handle, _axis, -5000);//设置负向软限位为
            res |= ZAux_Direct_SetFastDec(_handle, _axis, 4000); //设置快减速度
            res |= ZAux_Direct_SetOp(_handle, 16 + _axis * 2, 1); //打开使能 (OUT16.18.20.22.24.26.28.30)
            if (res == 0)
            {
                ShowMsg("初始化成功");
                // 启用定时器
                _slowTimer.Enabled = true;
                _fastTimer.Enabled = true;
            }
            else
            {
                ShowMsg("初始化失败");
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
                    if (string.IsNullOrEmpty(this.selectIP))
                    {
                        ShowMsg("未选择IP地址！");
                        return;
                    }
                    int ret = await Task.Run(() => Zmcaux.ZAux_OpenEth(this.selectIP, out _handle));
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
                this.ipAddressList.Clear();
                this.ipAddressList.Add("127.0.0.1");//模拟控制器地址
                string[] ethlist = builder.ToString().Trim().Split(' ');
                foreach (string eth in ethlist)
                {
                    this.ipAddressList.Add(eth);
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
            messageQueue.Clear();
            messageQueue.Enqueue(msg);
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

        private void OnFastTimedEvent(object sender, ElapsedEventArgs e)
        {
            int inMulti;
            ZAux_Direct_GetInMulti(_handle, 0, 31, out inMulti); //获取多路In
            _ioStatus.UpdateIO(inMulti);
        }
    }
}
