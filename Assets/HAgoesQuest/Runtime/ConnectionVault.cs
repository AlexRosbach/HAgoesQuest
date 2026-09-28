using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace HAgoesQuest
{
    // Only the ciphertext is written to storage. Android owns the non-exportable key.
    public static class ConnectionVault
    {
        const string Alias = "HAgoesQuest.connection.v1";
        [Serializable] sealed class Envelope { public string iv; public string ciphertext; }
        static string PathName => Path.Combine(Application.persistentDataPath, "connection.secure.json");
        public static bool Exists => File.Exists(PathName);

        public static ConnectionSettings Read()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var envelope = JsonUtility.FromJson<Envelope>(File.ReadAllText(PathName));
            using (var key = GetKey(false))
            using (var cipherClass = new AndroidJavaClass("javax.crypto.Cipher"))
            using (var cipher = cipherClass.CallStatic<AndroidJavaObject>("getInstance", "AES/GCM/NoPadding"))
            using (var spec = new AndroidJavaObject("javax.crypto.spec.GCMParameterSpec", 128, Convert.FromBase64String(envelope.iv)))
            {
                cipher.Call("init", 2, key, spec);
                byte[] plain = cipher.Call<byte[]>("doFinal", Convert.FromBase64String(envelope.ciphertext));
                return JsonUtility.FromJson<ConnectionSettings>(Encoding.UTF8.GetString(plain));
            }
#else
            throw new PlatformNotSupportedException("Persistent credentials are supported on the Quest.");
#endif
        }

        public static bool Save(ConnectionSettings settings)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var key = GetKey())
            using (var cipherClass = new AndroidJavaClass("javax.crypto.Cipher"))
            using (var cipher = cipherClass.CallStatic<AndroidJavaObject>("getInstance", "AES/GCM/NoPadding"))
            {
                cipher.Call("init", 1, key);
                byte[] encoded = cipher.Call<byte[]>("doFinal", Encoding.UTF8.GetBytes(JsonUtility.ToJson(settings)));
                var envelope = new Envelope { iv = Convert.ToBase64String(cipher.Call<byte[]>("getIV")), ciphertext = Convert.ToBase64String(encoded) };
                string temporary = PathName + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(envelope));
                if (File.Exists(PathName)) File.Replace(temporary, PathName, null);
                else File.Move(temporary, PathName);
                return true;
            }
#else
            // Editor sessions intentionally do not save access tokens to the project.
            return false;
#endif
        }

        public static void Forget()
        {
            if (File.Exists(PathName)) File.Delete(PathName);
            string legacy = Path.Combine(Application.persistentDataPath, "connection.json");
            if (File.Exists(legacy)) File.Delete(legacy);
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var storeClass = new AndroidJavaClass("java.security.KeyStore"))
            using (var store = storeClass.CallStatic<AndroidJavaObject>("getInstance", "AndroidKeyStore"))
            {
                store.Call("load", new object[] { null });
                store.Call("deleteEntry", Alias);
            }
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        static AndroidJavaObject GetKey(bool create = true)
        {
            using (var storeClass = new AndroidJavaClass("java.security.KeyStore"))
            using (var store = storeClass.CallStatic<AndroidJavaObject>("getInstance", "AndroidKeyStore"))
            {
                store.Call("load", new object[] { null });
                if (store.Call<bool>("containsAlias", Alias)) return store.Call<AndroidJavaObject>("getKey", Alias, null);
                if (!create) throw new InvalidOperationException("Saved connection key is missing.");
                using (var generatorClass = new AndroidJavaClass("javax.crypto.KeyGenerator"))
                using (var generator = generatorClass.CallStatic<AndroidJavaObject>("getInstance", "AES", "AndroidKeyStore"))
                using (var builder = new AndroidJavaObject("android.security.keystore.KeyGenParameterSpec$Builder", Alias, 3))
                {
                    using (builder.Call<AndroidJavaObject>("setBlockModes", new object[] { new string[] { "GCM" } })) { }
                    using (builder.Call<AndroidJavaObject>("setEncryptionPaddings", new object[] { new string[] { "NoPadding" } })) { }
                    using (var spec = builder.Call<AndroidJavaObject>("build")) generator.Call("init", spec);
                    return generator.Call<AndroidJavaObject>("generateKey");
                }
            }
        }
#endif
    }
}
