using System;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using ChatdollKit.Model;

namespace ChatdollKit.OSC
{
    public class OscOutputController : MonoBehaviour
    {
        public string oscTargetIp = "127.0.0.1";
        public int oscTargetPort = 9000;

        [Header("OSC Addresses")]
        public string chatboxAddress = "/chatbox/input";
        public string visemeAddress = "/avatar/parameters/Viseme";
        public string emotionAddressPrefix = "/avatar/parameters/Emotion";

        [Header("Settings")]
        public bool sendTextToChatbox = true;
        public bool sendVisemeData = true;
        public bool sendEmotionData = true;

        // Dependencies
        private UdpClient udpClient;
        private ModelController modelController;
        private SpeechController speechController;
        private AIAvatar aiAvatar;

        private void Awake()
        {
            udpClient = new UdpClient();
            modelController = GetComponent<ModelController>();
            if (modelController == null)
            {
                aiAvatar = GetComponent<AIAvatar>();
                if (aiAvatar != null)
                {
                    modelController = aiAvatar.ModelController;
                }
            }
        }

        private void Start()
        {
            if (modelController != null && modelController.SpeechController != null)
            {
                speechController = modelController.SpeechController;

                // Hook into text output (from SpeechController or DialogProcessor)
                speechController.OnSayStart += HandleSayStart;

                // Hook into audio samples for generic volume-based viseme
                if (sendVisemeData)
                {
                    speechController.HandlePlayingSamples += HandlePlayingSamples;
                }
            }
        }

        private void OnDestroy()
        {
            if (speechController != null)
            {
                speechController.OnSayStart -= HandleSayStart;
                speechController.HandlePlayingSamples -= HandlePlayingSamples;
            }
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient.Dispose();
            }
        }

        private void HandleSayStart(Voice voice, System.Threading.CancellationToken token)
        {
            if (sendTextToChatbox && !string.IsNullOrEmpty(voice.Text))
            {
                SendChatboxMessage(voice.Text);
            }
        }

        private void HandlePlayingSamples(float[] samples)
        {
            if (!sendVisemeData || samples == null || samples.Length == 0) return;

            // Simple RMS volume calculation for viseme (0.0 to 1.0)
            float sum = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                sum += samples[i] * samples[i];
            }
            float rms = Mathf.Sqrt(sum / samples.Length);

            // Send volume as viseme param
            SendOscMessage(visemeAddress, Mathf.Clamp01(rms * 10f)); // scale it up slightly for visibility
        }

        private string lastFaceName = "";

        private void Update()
        {
            if (sendEmotionData && modelController != null && modelController.FaceController != null)
            {
                var face = modelController.FaceController.GetFaceExpression();
                if (face != null && face.Name != lastFaceName)
                {
                    lastFaceName = face.Name;
                    SendOscMessage(emotionAddressPrefix + face.Name, true);
                }
            }
        }

        public void SendChatboxMessage(string text, bool typingIndicator = true, bool bypassPlaybook = false)
        {
            try
            {
                byte[] data = VRChatV2OscMessageBuilder.BuildChatboxMessage(text, typingIndicator, bypassPlaybook);
                udpClient.Send(data, data.Length, oscTargetIp, oscTargetPort);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error sending OSC chatbox message: {ex.Message}");
            }
        }

        public void SendOscMessage(string address, object value)
        {
            try
            {
                byte[] data = OscMessageBuilder.BuildMessage(address, value);
                udpClient.Send(data, data.Length, oscTargetIp, oscTargetPort);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error sending OSC message to {address}: {ex.Message}");
            }
        }
    }
}
