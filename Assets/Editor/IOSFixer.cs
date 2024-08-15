#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Editor
{
    public class IOSFixer
    {
        [PostProcessBuild(49)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string targetPath)
        {
            if (buildTarget == BuildTarget.iOS)
                ConfigureSwiftBuild(targetPath);
        }

        private static void ConfigureSwiftBuild(string targetPath)
        {
            var projPath = PBXProject.GetPBXProjectPath(targetPath);
            var proj = new PBXProject();

            proj.ReadFromString(File.ReadAllText(projPath));

            string targetGuid = proj.GetUnityFrameworkTargetGuid();
            string iphoneGuid = proj.GetUnityMainTargetGuid();

            proj.SetBuildProperty(targetGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
            proj.SetBuildProperty(iphoneGuid, "ENABLE_BITCODE", "YES");

            proj.SetBuildProperty(targetGuid, "SWIFT_VERSION", "5.0");
            proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "YES");
            proj.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            proj.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-ld64");

            string plistPath = Path.Combine(targetPath, "Info.plist");

            PlistDocument plist = new PlistDocument(); // Read Info.plist file into memory
            plist.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict rootDict = plist.root;
            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            rootDict.SetString("NSUserTrackingUsageDescription", "This identifier will be used to deliver personalized ads to you.");
            //rootDict.values.Remove("NSLocationWhenInUseUsageDescription");

            File.WriteAllText(plistPath, plist.WriteToString()); // Override Info.plist


            string podFilePath = Path.Combine(targetPath, "Podfile");
            if (File.Exists(podFilePath))
            {
                string podFileContents = File.ReadAllText(podFilePath);
                string targetUnityiPhone = "\ntarget 'Unity-iPhone' do";
                string inheritSearchPaths = "\n inherit! :search_paths";

                if (podFileContents.Contains("Unity-iPhone"))
                {
                    podFileContents = podFileContents.Replace(targetUnityiPhone, targetUnityiPhone + inheritSearchPaths);
                    File.WriteAllText(podFilePath, podFileContents);
                }

                File.AppendAllText(podFilePath, "# post install\npost_install do |installer|\n  # fix xcode 15 DT_TOOLCHAIN_DIR - remove after fix oficially - https://github.com/CocoaPods/CocoaPods/issues/12065\n  installer.aggregate_targets.each do |target|\n      target.xcconfigs.each do |variant, xcconfig|\n      xcconfig_path = target.client_root + target.xcconfig_relative_path(variant)\n      IO.write(xcconfig_path, IO.read(xcconfig_path).gsub(\"DT_TOOLCHAIN_DIR\", \"TOOLCHAIN_DIR\"))\n      end\n  end\n\n  installer.pods_project.targets.each do |target|\n    target.build_configurations.each do |config|\n      if config.base_configuration_reference.is_a? Xcodeproj::Project::Object::PBXFileReference\n          xcconfig_path = config.base_configuration_reference.real_path\n          IO.write(xcconfig_path, IO.read(xcconfig_path).gsub(\"DT_TOOLCHAIN_DIR\", \"TOOLCHAIN_DIR\"))\n      end\n    end\n  end\nend");
            }

            File.WriteAllText(projPath, proj.WriteToString());
        }

    }
}
#endif