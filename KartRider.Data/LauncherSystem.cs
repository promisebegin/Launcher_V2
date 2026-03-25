using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using KartRider.IO.Packet;

namespace KartRider
{
	public static class LauncherSystem
	{
		public static void MessageBoxType1()
		{
			MessageBox.Show("跑跑卡丁车已经运行了！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void MessageBoxType2()
		{
			MessageBox.Show("已经有一个启动器在运行了！\n不可以同时运行多个启动器！\n点击确认退出程序", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			Environment.Exit(1);
		}

		public static void MessageBoxType3()
		{
			MessageBox.Show("找不到KartRider.exe或KartRider.pin文件！\n点击确认退出程序", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			Environment.Exit(1);
		}

		public static GameVersion GetGameVersion()
		{
			byte[] data = RouterListener.Connect();
			string filePath = JsonHelper.GetFilePath();

			if (data != null)
			{
				InPacket iPacket = new InPacket(data);
				iPacket.ReadUInt();
				iPacket.ReadUInt();
				iPacket.ReadUShort();
				iPacket.ReadUShort();
				ushort version = iPacket.ReadUShort();
				string updateUrl = iPacket.ReadString();
				return new GameVersion { Version = version, UpdateUrl = updateUrl };
			}
			return null;
		}

		public static void CheckGame(string kartRiderDirectory)
		{
			string filePath = JsonHelper.GetFilePath();
			GameVersion version = LauncherSystem.GetGameVersion();
			if (version != null)
			{
				System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName("KartRider");
				foreach (System.Diagnostics.Process p in process)
				{
					p.Kill();
				}
				try
				{
					ProcessStartInfo startInfo = new ProcessStartInfo("Patcher.exe", $"'1' '123' '{version.UpdateUrl}/{version.Version}' '{kartRiderDirectory}' '{filePath}'")
					{
						WorkingDirectory = Path.GetFullPath(kartRiderDirectory),
						UseShellExecute = true,
						Verb = "runas" // 请求管理员权限（内存修改可能需要）
					};
					Process.Start(startInfo);
					Environment.Exit(0);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"操作失败：{ex.Message}");
				}
			}
			else
			{
				MessageBox.Show("获取游戏版本失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}

	public class GameVersion
	{
		public ushort Version { get; set; }
		public string UpdateUrl { get; set; }
	}
}