using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace TVTK
{
    class AudioOutput
    {
        //Попытка сделать выбор аудиовыхода    string audioSelector = MediaDevice.GetAudioRenderSelector();//выбор выхода аудио
        //Попытка сделать выбор аудиовыхода    var outputDevices = await DeviceInformation.FindAllAsync(audioSelector);
        //foreach (var device in outputDevices)
        //{
        //    var deviceItem = new ComboBoxItem();
        //    deviceItem.Content = device.Name;
        //    deviceItem.Tag = device;
        //    cmbbxAudioOutput.Items.Add(deviceItem);
        //}
        public static void GetAudio() 
        {

            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            MMDeviceCollection collection = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            MMDevice captureDevice = collection.FirstOrDefault();//.ItemAt(2);
            //captureDevice.


            //_capture = new WasapiLoopbackCapture(100, new WaveFormat(deviceFormat.SampleRate, deviceFormat.BitsPerSample, i));
            //_capture.Initialize();


            //capture.Device = captureDevice;
            ////before
            //_capture.Initialize();





            var q = new WaveOut();
            
            using (var devices = new MMDeviceEnumerator())
            {

                
                //devices.
                //devices.GetDefaultAudioEndpoint.SetAudio();

                //var y= devices.HasDefaultAudioEndpoint(DataFlow.All, Role.Multimedia);
                //var t = devices.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
               
                //foreach (var device in t)
                //{
                //    var t = device.AudioClient;
                    

                //    // do something with device
                //}
            }
        }
        public void SetAudio() { }
    }
}
