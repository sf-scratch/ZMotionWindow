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
using ZMotionWindow.Behaviors;
using ZMotionWindow.Extensions;
using ZMotionWindow.Models;
using ZMotionWindow.ZMotion;
using ZMotionWindow.ZMotion.enums;
using ZMotionWindow.ZMotion.exceptions;
using static cszmcaux.Zmcaux;

namespace ZMotionWindow.ViewModels
{
    internal class MainViewModel : BindableBase, IValidationExceptionHandler
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
        public DelegateCommand PauseMovingCommand { get; set; }
        public DelegateCommand ResumeMovingCommand { get; set; }
        public DelegateCommand ReturnZeroMotionCommand { get; set; }
        public DelegateCommand<OutStatusPanel> ChangeOutStatusCommand { get; set; }

        public SnackbarMessageQueue MessageQueue { get; set; }
        public ObservableCollection<string> IpAddressList { get; set; }
        public ObservableCollection<InStatusPanel> InStatusPanels { get; set; }
        public ObservableCollection<OutStatusPanel> OutStatusPanels { get; set; }

        private Timer _slowTimer;
        private IntPtr _handle;
        private int _axis;//轴号
        private int _fwdIn;//正向软限位IO口
        private int _revIn;//负向软限位IO口
        private int _datumIn;//设置原点位映射的IO口
        private ZmotionStatus _zmotionStatus;

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

        private float _startingSpeed;
        /// <summary>
        /// 轴起始速度
        /// </summary>
        public float StartingSpeed
        {
            get { return _startingSpeed; }
            set
            {
                _startingSpeed = value;
                RaisePropertyChanged();
            }
        }

        private float _runningSpeed;
        /// <summary>
        /// 轴速度
        /// </summary>
        public float RunningSpeed
        {
            get { return _runningSpeed; }
            set
            {
                _runningSpeed = value;
                RaisePropertyChanged();
            }
        }

        private float _acceleration;
        /// <summary>
        /// 轴加速度
        /// </summary>
        public float Acceleration
        {
            get { return _acceleration; }
            set
            {
                _acceleration = value;
                RaisePropertyChanged();
            }
        }

        private float _deceleration;
        /// <summary>
        /// 轴减速度
        /// </summary>
        public float Deceleration
        {
            get { return _deceleration; }
            set
            {
                _deceleration = value;
                RaisePropertyChanged();
            }
        }

        private float _units;
        /// <summary>
        /// 脉冲当量
        /// </summary>
        public float Units
        {
            get { return _units; }
            set
            {
                _units = value;
                RaisePropertyChanged();
            }
        }

        private float _sramp;
        /// <summary>
        /// 轴的S曲线时间
        /// </summary>
        public float Sramp
        {
            get { return _sramp; }
            set
            {
                _sramp = value;
                RaisePropertyChanged();
            }
        }

        private float _inchMoveDistance;
        /// <summary>
        /// 寸动距离
        /// </summary>
        public float InchMoveDistance
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

        private string _moveTypeStr;
        /// <summary>
        /// 运动状态
        /// </summary>
        public string MoveTypeStr
        {
            get { return _moveTypeStr; }
            set
            {
                _moveTypeStr = value;
                RaisePropertyChanged();
            }
        }

        private bool _windowIsEnable;

        public bool WindowIsEnable
        {
            get { return _windowIsEnable; }
            set
            {
                _windowIsEnable = value;
                RaisePropertyChanged();
            }
        }


        /// <summary>
        /// 输入的数据是否全部有效
        /// </summary>
        public bool IsAllValid { get ; set ; }

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
            this.PauseMovingCommand = new DelegateCommand(PauseMoving);
            this.ResumeMovingCommand = new DelegateCommand(ResumeMoving);
            this.ReturnZeroMotionCommand = new DelegateCommand(ReturnZeroMotion);
            this.ChangeOutStatusCommand = new DelegateCommand<OutStatusPanel>(ChangeOutStatus);
            this._isConnect = false;
            this._isConnectEnable = true;
            this.IpAddressList = new ObservableCollection<string>();
            this.InStatusPanels = new ObservableCollection<InStatusPanel>();
            this.OutStatusPanels = new ObservableCollection<OutStatusPanel>();
            this.MessageQueue = new SnackbarMessageQueue();
            this.StartingSpeed = 0;
            this.RunningSpeed = 50;
            this.Acceleration = 100;
            this.Deceleration = 100;
            this.Units = 200;
            this.Sramp = 200;
            this.InchMoveDistance = 100;
            this.RelativeMove = true;
            this.ForwardDirection = true;
            this.IsAllValid = true;
            _windowIsEnable = true;
            _slowTimer = new Timer(100);
            _slowTimer.Elapsed += OnSlowTimedEvent;
            _fwdIn = 4;
            _revIn = 2;
            _datumIn = 0;
            _axis = 2;
        }

        private void ChangeOutStatus(OutStatusPanel curPanel)
        {
            ZAux_Direct_SetOutMulti(_handle, (ushort)curPanel.OutNum, (ushort)curPanel.OutNum, new uint[] { curPanel.OutStatus == 0 ? 1U : 0U });
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



            WindowIsEnable = false;
            try
            {
                await CustomZMotion.ReturnZeroAsync(_handle, _axis, _zmotionStatus, _fwdIn, _revIn, _datumIn);
                ZAux_Direct_SetFsLimit(_handle, _axis, 5000);//设置正向软限位为
                ZAux_Direct_SetRsLimit(_handle, _axis, -5000);//设置负向软限位为
            }
            catch (ZMotionStopException ex)
            {
                ShowMsg("回零中止！");
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
            SetMotionParam();
            //int ret = ZAux_Direct_SetCreep(_handle, _axis, 50);
            //ret |= ZAux_Direct_SetDatumIn(_handle, _axis, 0); //设置原点点开关
            //ret |= ZAux_Direct_SetHomeWait(_handle, _axis, 1000); //设置回零等待时间
            //ret |= ZAux_Direct_Single_Datum(_handle, _axis, 3); //回零，模式 3
            //await Task.Run(async () =>
            //{
            //    uint homestatus = 0;
            //    while (true)//等待轴 0 回零运动完成
            //    {
            //        await Task.Delay(100);
            //        ZAux_Direct_GetHomeStatus(_handle, _axis, ref homestatus);//获取回零运动完成状态
            //        if (homestatus == 1) break;
            //    }
            //});
            WindowIsEnable = true;
        }

        private void ResumeMoving()
        {
            int res = ZAux_Direct_MoveResume(_handle, _axis);
            ShowMsg(res == 0 ? "继续成功！" : "继续失败！");
        }

        private void PauseMoving()
        {
            int res = ZAux_Direct_MovePause(_handle, _axis, 3);
            ShowMsg(res == 0 ? "暂停成功！" : "暂停失败！");
        }

        private async void StopMoving()
        {
            try
            {
                int res = await CustomZMotion.StopAsync(_handle, _axis, _zmotionStatus);
                ShowMsg(res == 0 ? "停止成功！" : "停止失败！");
            }
            catch(ZMotionStopTimeOutException ex)
            {
                ShowMsg(ex.Message);
            }
            catch(Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }

        private void ResetPosition()
        {
            int res = ZAux_Direct_SetDpos(_handle, _axis, 0);
            res = ZAux_Direct_SetMpos(_handle, _axis, 0);
            ShowMsg(res == 0 ? "位置清零成功！" : "位置清零失败！");
        }

        private void InchMove()
        {
            if (SetMotionParam() == 0)
            {
                int res = 0;
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
        }

        private void OpenOscilloscope()
        {
            int res = Zmcaux.ZAux_Trigger(_handle);//示波器触发函数
            ShowMsg(res == 0 ? "成功触发示波器！" : "触发示波器失败！");
        }

        private void PositiveMove()
        {
            if (SetMotionParam() == 0)
            {
                int res = 0;
                res |= ZAux_Direct_Single_Vmove(_handle, _axis, 1);
                ShowMsg(res == 0 ? "开始正向运动" : "开始正向失败！");
            }
        }

        private void NegativeMove()
        {
            if (SetMotionParam() == 0)
            {
                int res = 0;
                res |= ZAux_Direct_Single_Vmove(_handle, _axis, -1);
                ShowMsg(res == 0 ? "开始反向运动" : "开始反向失败！");
            }
        }

        private int SetMotionParam()
        {
            int res = 0;
            if (IsAllValid)
            {
                res = CustomZMotion.SetMotionParam(_handle, _axis, StartingSpeed, RunningSpeed, Acceleration, Deceleration, Units, Sramp);
                if (res != 0)
                {
                    ShowMsg("设置运动参数出现异常");
                }
            }
            else
            {
                ShowMsg("输入的运动参数格式有误");
                res = -1;
            }
            return res;
        }

        private void InitControl()
        {
            int res = 0;
            res |= ZAux_Direct_SetAtype(_handle, _axis, 4);//设置轴类型
            res |= ZAux_Direct_SetFwdIn(_handle, _axis, _fwdIn);//设置正向软限位IO口
            res |= ZAux_Direct_SetRevIn(_handle, _axis, _revIn);//设置负向软限位IO口
            res |= ZAux_Direct_SetDatumIn(_handle, _axis, _datumIn);//设置原点位映射的IO口
            res |= ZAux_Direct_SetFastDec(_handle, _axis, 4000); //设置快减速度
            res |= ZAux_Direct_SetOp(_handle, 16 + _axis * 2, 1); //打开使能 (OUT16.18.20.22.24.26.28.30)
            if (res == 0)
            {
                InitIOStatusPanels();
                // 启用定时器
                _slowTimer.Enabled = true;
                _zmotionStatus?.Dispose();
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
            InStatusPanels.Clear();
            OutStatusPanels.Clear();
            int len = sizeof(long) * 8;
            long inMulti = CustomZMotion.GetInMulti0_63(_handle);
            for (int inNum = 0; inNum < len; inNum++)
            {
                InStatusPanels.Add(new InStatusPanel(inMulti & (1L << inNum), inNum));
            }
            long outMulti = CustomZMotion.GetOutMulti0_63(_handle);
            for (int outNum = 0; outNum < len; outNum++)
            {
                OutStatusPanels.Add(new OutStatusPanel(outMulti & (1L << outNum), outNum));
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
                this.IpAddressList.Clear();
                this.IpAddressList.Add("127.0.0.1");//本地仿真控制器地址
                string[] ethlist = builder.ToString().Trim().Split(' ');
                foreach (string eth in ethlist)
                {
                    this.IpAddressList.Add(eth);
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
            MessageQueue.Clear();
            MessageQueue.Enqueue(msg);
        }

        private void OnSlowTimedEvent(object sender, ElapsedEventArgs e)
        {
            int res = 0, axisStatus = 0, moveType = 0;
            float dpos = 0, mpos = 0;
            res |= ZAux_Direct_GetDpos(_handle, _axis, ref dpos);
            res |= ZAux_Direct_GetMpos(_handle, _axis, ref mpos);
            res |= ZAux_Direct_GetAxisStatus(_handle, _axis, ref axisStatus); //获取轴状态
            res |= ZAux_Direct_GetMtype(_handle, _axis, ref moveType); //获取运动状态
            InstructDpos = dpos;
            BackDpos = mpos;
            if (axisStatus == 0)
            {
                AxisStatusStr = "正常";
            }
            else
            {
                List<string> statusList = new List<string>();
                foreach (AxisStatus status in Enum.GetValues(typeof(AxisStatus)))
                {
                    if ((axisStatus & (int)status) != 0)
                    {
                        statusList.Add(status.GetEnumDescription());
                    }
                }
                AxisStatusStr = string.Join(", ", statusList.ToArray());
            }

            MoveTypeStr = ((MoveType)moveType).GetEnumDescription();
        }

        private void ZmotionStatus_InUpdatedEvent(long inMulti)
        {
            int len = sizeof(long) * 8;
            for (int inNum = 0; inNum < len; inNum++)
            {
                InStatusPanels[inNum].InStatus = inMulti & (1L << inNum);
            }
        }

        private void ZmotionStatus_OutUpdatedEvent(long outMulti)
        {
            int len = sizeof(long) * 8;
            for (int outNum = 0; outNum < len; outNum++)
            {
                OutStatusPanels[outNum].OutStatus = outMulti & (1L << outNum);
            }
        }
    }
}
