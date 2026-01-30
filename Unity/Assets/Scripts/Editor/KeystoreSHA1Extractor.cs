using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;

/// <summary>
/// Android Keystore의 SHA-1 지문을 추출하는 에디터 도구
/// </summary>
public class KeystoreSHA1Extractor : EditorWindow
{
    [MenuItem("Tools/Get SHA-1")]
    public static void ExtractSHA1()
    {
        // 현재 프로젝트에서 설정된 Keystore 정보 가져오기
        string keystorePath = PlayerSettings.Android.keystoreName;
        string keystorePass = PlayerSettings.Android.keystorePass;
        string keyaliasName = PlayerSettings.Android.keyaliasName;
        string keyaliasPass = PlayerSettings.Android.keyaliasPass;

        // Keystore 경로가 설정되어 있지 않으면 디버그 키스토어 사용
        if (string.IsNullOrEmpty(keystorePath))
        {
            UnityEngine.Debug.LogWarning("프로젝트에 Keystore가 설정되어 있지 않습니다. 디버그 키스토어를 사용합니다.");

            // 디버그 키스토어 경로 설정
            string userProfile = System.Environment.GetEnvironmentVariable("USERPROFILE");
            if (string.IsNullOrEmpty(userProfile))
            {
                userProfile = System.Environment.GetEnvironmentVariable("HOME"); // Mac/Linux
            }

            keystorePath = Path.Combine(userProfile, ".android", "debug.keystore");
            keystorePass = "android";
            keyaliasName = "androiddebugkey";
            keyaliasPass = "android";

            UnityEngine.Debug.Log($"디버그 키스토어 경로: {keystorePath}");
        }

        // Keystore 파일이 존재하는지 확인
        if (!File.Exists(keystorePath))
        {
            UnityEngine.Debug.LogError($"Keystore 파일을 찾을 수 없습니다: {keystorePath}");
            return;
        }

        // keytool 명령어 실행
        string keytoolPath = FindKeytool();
        if (string.IsNullOrEmpty(keytoolPath))
        {
            UnityEngine.Debug.LogError("keytool을 찾을 수 없습니다. JDK가 설치되어 있는지 확인하세요.");
            return;
        }

        UnityEngine.Debug.Log("SHA-1 지문을 추출하는 중...");

        // keytool 명령어 구성
        string arguments = $"-list -v -keystore \"{keystorePath}\" -alias {keyaliasName} -storepass {keystorePass} -keypass {keyaliasPass}";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = keytoolPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        try
        {
            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error) && process.ExitCode != 0)
                {
                    UnityEngine.Debug.LogError($"SHA-1 추출 실패:\n{error}");
                    return;
                }

                // SHA-1 지문 파싱
                string[] lines = output.Split('\n');
                string sha1 = "";
                string sha256 = "";
                string md5 = "";

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("SHA1:"))
                    {
                        sha1 = trimmedLine.Substring(5).Trim();
                    }
                    else if (trimmedLine.StartsWith("SHA256:"))
                    {
                        sha256 = trimmedLine.Substring(7).Trim();
                    }
                    else if (trimmedLine.StartsWith("MD5:"))
                    {
                        md5 = trimmedLine.Substring(4).Trim();
                    }
                }

                // 결과 출력
                UnityEngine.Debug.Log("====================================");
                UnityEngine.Debug.Log($"<color=cyan><b>Keystore:</b></color> {keystorePath}");
                UnityEngine.Debug.Log($"<color=cyan><b>Alias:</b></color> {keyaliasName}");
                UnityEngine.Debug.Log("====================================");

                if (!string.IsNullOrEmpty(sha1))
                {
                    UnityEngine.Debug.Log($"<color=green><b>SHA-1:</b></color> {sha1}");
                }

                if (!string.IsNullOrEmpty(sha256))
                {
                    UnityEngine.Debug.Log($"<color=green><b>SHA-256:</b></color> {sha256}");
                }

                if (!string.IsNullOrEmpty(md5))
                {
                    UnityEngine.Debug.Log($"<color=green><b>MD5:</b></color> {md5}");
                }

                UnityEngine.Debug.Log("====================================");

                // 전체 출력도 표시 (필요시)
                // UnityEngine.Debug.Log($"전체 출력:\n{output}");
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"SHA-1 추출 중 오류 발생: {ex.Message}");
        }
    }

    /// <summary>
    /// keytool 실행 파일 경로 찾기
    /// </summary>
    private static string FindKeytool()
    {
        // Unity의 JDK 경로 확인
        string jdkPath = EditorPrefs.GetString("JdkPath");
        if (!string.IsNullOrEmpty(jdkPath))
        {
            string keytoolPath = Path.Combine(jdkPath, "bin", "keytool.exe");
            if (File.Exists(keytoolPath))
            {
                return keytoolPath;
            }

            // Mac/Linux용 (확장자 없음)
            keytoolPath = Path.Combine(jdkPath, "bin", "keytool");
            if (File.Exists(keytoolPath))
            {
                return keytoolPath;
            }
        }

        // JAVA_HOME 환경 변수 확인
        string javaHome = System.Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome))
        {
            string keytoolPath = Path.Combine(javaHome, "bin", "keytool.exe");
            if (File.Exists(keytoolPath))
            {
                return keytoolPath;
            }

            keytoolPath = Path.Combine(javaHome, "bin", "keytool");
            if (File.Exists(keytoolPath))
            {
                return keytoolPath;
            }
        }

        // PATH 환경 변수에서 찾기 (Windows)
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "where",
                Arguments = "keytool",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                {
                    // 첫 번째 경로 반환
                    string[] paths = output.Split('\n');
                    if (paths.Length > 0)
                    {
                        return paths[0].Trim();
                    }
                }
            }
        }
        catch
        {
            // Windows가 아니거나 where 명령어가 없는 경우
        }

        // which 명령어로 시도 (Mac/Linux)
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = "keytool",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                {
                    return output;
                }
            }
        }
        catch
        {
            // which 명령어 사용 불가
        }

        // 그냥 "keytool"을 반환하고 시스템 PATH에 있기를 기대
        return "keytool";
    }
}
