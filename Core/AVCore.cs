using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static FFmpeg.AutoGen.ffmpeg;



namespace OmniBuildtest.Core
{
         public static class AvUtil 
          {
                  public static void checkprint(int error, string message, string mode = "DEBUG", string level = "DEBUG")
                    {
                              if (error < 0)
                              {
                                        switch (level)
                                        {
                                                  case "DEBUG":
                                                            if (mode == "INFO")
                                                            {
                                                                      Console.WriteLine("\t[INFO]  :\t" + message + $" : {printaverror(error)}");
                                                            }
                                                            else if (mode == "DEBUG")
                                                            {
                                                                      Console.WriteLine("\t[DEBUG]  :\t" + message + $" : {printaverror(error)}");
                                                            }
                                                            break;
                                                  case "INFO":
                                                            if (mode == "INFO")
                                                            {
                                                                      Console.WriteLine("\t[INFO]  :\t" + message + $" : {printaverror(error)}");
                                                            }
                                                            break;
                                                  default: break;
                                        }
                                        Console.WriteLine(message + $" : {printaverror(error)}");

                              }

                    }
                 public   static unsafe string printaverror(int averrorcode)
                    {
                              byte* text = (byte*)av_malloc(AV_ERROR_MAX_STRING_SIZE);
                              byte[] array = new byte[AV_ERROR_MAX_STRING_SIZE];
                              av_make_error_string(text, AV_ERROR_MAX_STRING_SIZE, averrorcode);
                              Marshal.Copy((IntPtr)text, array, 0, AV_ERROR_MAX_STRING_SIZE);
                              return System.Text.Encoding.UTF8.GetString(array);
                    }

          }
          public enum CodecType 
          {
                    Unknown = 0,
                    Decoder=1,
                    Encoder=2,
          }
          public unsafe class AvCodec :IDisposable
          {
                      AVCodec* codec { get; set; }
                    public CodecType codectype { get; set; }
                    public AvCodec() 
                    {
                              
                    }
                    public void Init(string name) 
                    {
                              codec=avcodec_find_decoder_by_name(name);
                    }
                    public AVCodec* Reference() 
                    {
                              if (codec == null) 
                              {
                                        throw new Exception("Codec Not Initialized !");
                              }
                              return codec;
                    }
                   
                    public void Dispose()
                                        {
                                              av_free(codec);
                                        }
          }
          public unsafe class AvCodecContext:IDisposable
          {
                    public AvCodec Codec { get; set; } = new AvCodec();
                    AVCodecContext* codeccontext = null;
                    int succ = 0;
                    public string Name { get; set; }
                    public Dictionary<string,string > Options { get; set; }
                    AVDictionary* options { get; set; } = null;
                    MediaType MediaType { get => codeccontext == null ? MediaType.Unkwon : codeccontext->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO ? MediaType.Video : codeccontext->codec_type== AVMediaType.AVMEDIA_TYPE_AUDIO ? MediaType.Audio:MediaType.Subtitles; }
                    
                    public string PixelFormat { get=>pixelformat; set=>pixelformat=value; }
                    string pixelformat { get=>av_get_pix_fmt_name(codeccontext->pix_fmt); set=>codeccontext->pix_fmt=av_get_pix_fmt(value); }

                    public int GOPSize { get => gopsize; set => gopsize = value; }
                    int gopsize { get => codeccontext->gop_size; set => codeccontext->gop_size = value; }

                    public int Width { get => width; set => width = value; }
                    int width { get => codeccontext->width; set => codeccontext->width = value; }

                    public int Height { get => height; set => height = value; }
                    int height { get => codeccontext->height; set => codeccontext->height = value; }


                    public long Bitrate { get => bitrate; set => bitrate = value; }
                    long bitrate { get =>codeccontext->bit_rate; set => codeccontext->bit_rate = value; }

                    public AVRational TimeBase { get => timebase; set => timebase = value; }
                    AVRational timebase { get => codeccontext->time_base; set => codeccontext->time_base = value; }

                    public AVRational FrameRate { get => framerate; set => framerate = value; }
                    AVRational framerate { get => codeccontext->framerate; set => codeccontext->framerate = value; }

                    public int SampleRate { get => samplerate; set => samplerate = value; }
                    int samplerate { get => codeccontext->sample_rate; set => codeccontext->sample_rate = value; }

                    public AVChannelLayout ChannelLayout { get => channellayout; set => channellayout = value; }
                    AVChannelLayout channellayout { get => codeccontext->ch_layout; set => codeccontext->ch_layout = value; }

                    public AVSampleFormat SampleFormat { get => sampleformat; set => sampleformat = value; }
                    AVSampleFormat sampleformat { get => codeccontext==null?new AVSampleFormat():codeccontext->sample_fmt; set => codeccontext->sample_fmt = value; }

                    public void SetChannelLayout(long bitrate= 128000, string layout="stereo") 
                    {
                              var chlyt = new AVChannelLayout();
                              succ = av_channel_layout_from_string(&chlyt, layout);
                             AvUtil.checkprint(succ, "Not initialized layout");
                              codeccontext->ch_layout = chlyt;
                              codeccontext->bit_rate = bitrate;

                    }

                    public bool IsInitialized { get => codeccontext == null; }
                    public AvCodecContext() 
                    {
                              Codec=new AvCodec();
                              
                    }
                    public void SetGlobalHeader() 
                    {
                              codeccontext->flags |= AV_CODEC_FLAG_GLOBAL_HEADER;
                    }
                    public void Setup() 
                    {
                              AVDictionary* dictopts = options;
                              foreach(string key in Options.Keys) 
                              {
                                        succ=av_dict_set(&dictopts, key, Options[key],0);
                                        AvUtil.checkprint(succ,$"Error Setting Options {key} of {Options[key]} |\t");
                              }
                    }
                    public void Accel() 
                    {
                              
                              av_opt_set(codeccontext->priv_data, "x264-params", " opencl=true", 0);
                    }
                    public bool Init(AvCodec codec) 
                    {
                              avcodec_alloc_context3(codec.Reference());
                              return true;
                    }
                    public bool CopyCodecParamsToStream(AvStream stream) 
                    {
                              succ=avcodec_parameters_from_context(stream.Reference()->codecpar, codeccontext);
                              return succ < 0 ? false: true;
                    }
                    public IntPtr ReadReference() 
                    {
                              return (IntPtr)codeccontext; 
                    }
                    public AVCodecContext* Reference()
                    {
                              return codeccontext;
                    }
                    public bool Open( ) 
                    {
                              AVDictionary *dictoptions=options;
                              succ=avcodec_open2(codeccontext,Codec.Reference(), &dictoptions);
                              AvUtil.checkprint(succ, "Error Opening Codec Context");
                              return true;
                    }
                    public bool Close() 
                    {
                              AVDictionary* dictoptions = options;
                              succ = avcodec_close(codeccontext);
                              AvUtil.checkprint(succ, "Error Closing Information");
                              return true;
                    }
                    public void Dispose()
                    {
                              AVCodecContext* context = codeccontext;
                              avcodec_free_context(&context);
                    }
          }
          public class AvDevice
          {
                    public string Name { get; set; } = string.Empty;
                    public MediaType Mediatype { get; set; } = MediaType.Unkwon;
          }
          public  unsafe class AvFormatContext:IDisposable
          {
                    public string URL { get; set; } = string.Empty;
                    int succ = 0;
                    public bool IsInput { get; set; } = new bool();
                    public bool HasInputFormat { get; set; } = false;
                    AVFormatContext* format { get; set; } = null;
                    AVInputFormat* inputformat = null;
                    AVDictionary* options = null;
                    public uint NumChapters { get=>format->nb_chapters;  }
                    public uint NumStreams { get; set; } = new uint();
                     public  List<AvStream> Streams = new List<AvStream>();
                    public   Dictionary<string,string > Options = new Dictionary<string,string>();
                    public List<AvDevice> Devices = new List<AvDevice>();
                    public void SetIntputFormat(string name) 
                    {
                              inputformat = av_find_input_format(name);

                    }
                    /// <summary>
                    /// Get streams and adds  them to Streams
                    /// </summary>
                    public void GetStreams() 
                    {
                            
                              foreach(int streamcount in Enumerable.Range(0,(int)this.NumStreams)) 
                              {
                                        AvStream stream = new AvStream();
                                        stream.StreamIndex = streamcount;
                                        stream.SetReference(&format->streams[streamcount]);
                                        stream.IsInput = false;
                                        Streams.Add(stream);


                              }

                    }
                    public void WriteTrailer() 
                    {
                              succ=av_write_trailer(format);
                              AvUtil.checkprint(succ, "Error Writing Trailer");
                    }
                    public void SetStream(AvCodec codec) 
                    {
                              AVStream* streamptr = avformat_new_stream(format,codec.Reference());
                              AvStream stream = new AvStream();
                              stream.IsInput = true;
                              stream.SetReference(&streamptr);
                              Streams.Add(stream);
                    }
                    public void WriteHeader()
                    {
                    }
                    public AvPacket Read() 
                    {
                              AvPacket packet=new AvPacket();
                              succ=av_read_frame(format, packet.Reference());
                              AvUtil.checkprint(succ, "Error");
                               return packet;
                    }

                    /// <summary>
                    /// Set the Dictionary<string,string> Options to AVDictionary inorder to open the Format
                    /// </summary>
                    public void Setup() 
                    {
                              AVDictionary* dictoptions = options;
                              foreach (string key in Options.Keys) 
                              {
                                        succ = av_dict_set(&dictoptions, key, Options[key], 0);
                                        AvUtil.checkprint(succ, "Error Setting Up Dictionary");
                              }
                    }
                    /// <summary>
                    /// Open the Device/File using inputformat 
                    /// Input Format must be set if a device is to be opened 
                    /// </summary>
                    public bool Open()
                    {
                              AVFormatContext* formatptr = format;
                              AVDictionary* dictoptions = options;
                              succ = avformat_open_input(&formatptr, URL, inputformat, &dictoptions);
                              AvUtil.checkprint(succ, "Error Opening Format Context");
                              /*
                               * Load Streams
                               */
                              GetStreams();
                              return succ < 0 ? false : true;
                    }
                    public void Close() 
                    {
                              AVFormatContext* formatptr = format;
                              avformat_close_input(&formatptr);
                    }
                    
                    public void Dispose()
                    {
                              avformat_free_context(format);
                    }

                    public AvFormatContext() 
                    {
                              format = avformat_alloc_context();
                              avdevice_register_all();
                              
                    }
                    public void GetDevices(string input) 
                    {
                              if (System.OperatingSystem.IsWindows()) 
                              {
                                        input = "dshow";

                              }
                              inputformat=av_find_input_format(input);
                              AVDeviceInfoList* deviceList;

                              avdevice_list_input_sources(inputformat, null, null, &deviceList);
                              // Print the device names
                              for (int i = 0; i < deviceList->nb_devices; i++)
                              {
                                        var device = deviceList->devices[i];
                                        string deviceName = Marshal.PtrToStringUTF8((IntPtr)device->device_description);
                                        AvDevice devicename = new AvDevice();
                                        devicename.Name = deviceName;
                                        
                                        devicename.Mediatype = device->media_types[0] == AVMediaType.AVMEDIA_TYPE_VIDEO ? MediaType.Video : device->media_types[0] == AVMediaType.AVMEDIA_TYPE_AUDIO ? MediaType.Audio : MediaType.Unkwon;
                                        Devices.Add(devicename);
                              }
                    }
          }
        
          public enum MediaType
          {
                    Video = 0,
                    Audio = 1,         
                    Unkwon=2,
                    Subtitles=3
          }
          public unsafe class AvCodecParameters 
          {
                    public MediaType Type { get; set; }
                    AVCodecParameters* parameters=null;
                    public string Name { get; set; } = string.Empty;
                    public string Codec { get; set; } = string.Empty;


                    public int Width { get => width; set => width = value; }
                    int width { get => parameters->width; set => parameters->width = value; }

                    public int Height { get => height; set => height = value; }
                    int height { get => parameters->height; set => parameters->height = value; }


                    public long Bitrate { get => bitrate; set => bitrate = value; }
                    long bitrate { get => parameters->bit_rate; set => parameters->bit_rate = value; }



                    public int SampleRate { get => samplerate; set => samplerate = value; }
                    int samplerate { get => parameters->sample_rate; set => parameters->sample_rate = value; }

                    public AVChannelLayout ChannelLayout { get => channellayout; set => channellayout = value; }
                    AVChannelLayout channellayout { get => parameters->ch_layout; set => parameters->ch_layout = value; }

                    public string PixelFormat { get => pixelformat; set => pixelformat = value; }
                    string pixelformat { get => parameters == null ?"" :av_get_pix_fmt_name((AVPixelFormat)parameters->format); set => parameters->format =(int) av_get_pix_fmt(value); }

                    public string SampleFormat { get => sampleformat; set => sampleformat = value; }
                    string sampleformat { get => parameters == null ? "" : av_get_sample_fmt_name((AVSampleFormat)parameters->format); set => parameters->format = (int)av_get_sample_fmt(value); }


                   
                    
                    public void SetReference(AVCodecParameters *parametersptr) 
                    {
                              parameters = parametersptr;
                    }

          }
          public unsafe class AvFilterContext:IDisposable
          {
                    AVFilter* filter = null;
                    int succ = 0;
                    IntPtr filtercontext = IntPtr.Zero;
                    string filterargs = string.Empty;
                    AVFilterGraph* filtergraph = null;
                    string filtername { get; set; } = string.Empty;
                    public void LinkToFilterContext(AvFilterContext context ,uint inputpad,uint outputpad) 
                    {
                              succ = avfilter_link((AVFilterContext*)filtercontext.ToPointer(), inputpad,context.ReferenceContext(), outputpad);
                              AvUtil.checkprint(succ, "Error Getting input");
                    }
                    public void SetArgs(string Args) 
                    {
                              filterargs = Args;
                    }

                    public void InitFilter(AvFilterGraph graph,string name="buffersrc",string fname="[filter]") 
                    {
                              filtername = name+"\t"+fname;
                              filter = avfilter_get_by_name(name);
                              AVFilterContext* filterctx=(AVFilterContext*)filtercontext.ToPointer();
                              succ = avfilter_graph_create_filter(&filterctx,filter, fname, filterargs, null, graph.Reference());
                              AvUtil.checkprint(succ, "Error Creating Audio Buffer Sink Filter");

                    }
                    public void SendFrame() 
                    {
                              AvFrame frame = new AvFrame();
                              succ=av_buffersrc_add_frame((AVFilterContext*)filtercontext.ToPointer(), frame.Reference());
                              AvUtil.checkprint(succ, $"Error Sending  Frames to filter {this.filtername}");
                    }
                    public AvFrame GetFrame() 
                    {
                              AvFrame frame = new AvFrame();
                              succ=av_buffersink_get_frame((AVFilterContext*)filtercontext.ToPointer(), frame.Reference());
                              AvUtil.checkprint(succ, $"Error Getting Frame from filter {this.filtername}\t");
                              return frame;
                            
                    }
                    public AVFilterContext* ReferenceContext() 
                    {
                              return (AVFilterContext*)filtercontext.ToPointer();
                    }
                    public AVFilter* ReferenceFilter() 
                    {
                              return filter;
                    }

                    public void Dispose()
                    {
                              avfilter_free((AVFilterContext*) filtercontext.ToPointer());
                              
                    }
          }

          public unsafe class AvFilterGraph :IDisposable
          {
                    AVFilterGraph* filtergraph = null;
                    int succ = 0;
                    public AvFilterGraph() 
                    {
                              filtergraph = avfilter_graph_alloc();


                    }
                    public void Configure() 
                    {
                              succ=avfilter_graph_config(filtergraph, null);
                              AvUtil.checkprint(succ, "Error Configuring FilterGraph");
                    }

                    public void Dispose()
                    {
                              AVFilterGraph* context = filtergraph;
                              avfilter_graph_free(&context);
                    }

                    public AVFilterGraph* Reference() 
                    {
                              return filtergraph;
                    }
          }
          public unsafe partial class AvStream : IDisposable
          {
                    public MediaType MediaType { get => stream->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO ? MediaType.Video : stream->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO ? MediaType.Audio : MediaType.Subtitles; }
                    int succ = 0;
                    AVStream* stream = null;
                    public int  StreamIndex = 0;
                    public AvCodecParameters CodecParameters { get; set; }=new AvCodecParameters();
                    public bool IsInput { get; set; } = new bool();
                    public void Dispose()
                    {
                              av_free(stream);
                              stream = null;
                              
                    }
                    public AVStream * Reference() 
                    {
                              return stream;
                    }
                    public void SetReference(AVStream** streamptr) 
                    {
                              stream = *streamptr;
                              
                              
                    }
                    public bool CopyParamsToCodecContext(AvCodecContext codecctx) 
                    {
                              succ = avcodec_parameters_to_context(codecctx.Reference(),stream->codecpar);
                              AvUtil.checkprint(succ, "Error Copying Stream Parameters to Context ");
                              return (succ < 0)?false:true;
                    }
          }
          public unsafe partial class AvPacket :IDisposable
          {
                    AVPacket* packet { get; set; }
                    public AvPacket()
                    {
                              packet = av_packet_alloc();
                    }
                    public void ReadReference(AVPacket ** refpacket) 
                    {
                              packet = *refpacket;
                    }
                    public AVPacket* Reference() 
                    {
                              return packet;
                    }

                    public void Dispose()
                    {
                            av_packet_unref(packet);
                    }
          }
          public unsafe  class AvFrame :IDisposable
          {
                    AVFrame* frame=null;
                    public long Pts { get=>frame->pts; set => frame->pts = value; }
                    public long Dts { get => frame->pkt_dts; set => frame->pkt_dts = value; }
                    public long Duration { get => frame->pkt_duration; set => frame->pkt_duration = value; }
                    public int SampleRate { get => frame->sample_rate; set => frame->sample_rate = value; }

                    public AVRational TimeBase { get => frame->time_base; set => frame->time_base = value; }

                    public int Width { get => frame->width; set => frame->width = value; }
                    public int Height { get => frame->height; set => frame->height = value; }

                    public int Channels { get => frame->channels; set => frame->channels = value; }
                    public AVChannelLayout ChannelLayout { get => frame->ch_layout; set => frame->ch_layout = value; }

                    public int KeyFrame { get => frame->key_frame;  }
                    

                    public string  PixelFormat { get => av_get_pix_fmt_name((AVPixelFormat)frame->format); set => frame->format = (int)av_get_pix_fmt( value); }
                    
                    public string SampleFormat { get => av_get_sample_fmt_name((AVSampleFormat)frame->format); set => frame->format = (int)av_get_sample_fmt(value); }


                    public object Data { get=>frame->data; }
                   
                    
                    public AvFrame() 
                    {
                              frame = av_frame_alloc();
                    }

                    public void Dispose()
                    {
                              av_frame_unref(frame);
                    }
                    public AVFrame* Reference() 
                    {
                              return frame;
                    }
                    public string ToImageString() 
                    {
                              string image=string.Empty;
                              var bitmap = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                              var bitmapData = bitmap.LockBits(new Rectangle(0, 0, frame->width, frame->height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                              int succ=sws_scale(
                              sws_getCachedContext(null, frame->width, frame->height, av_get_pix_fmt(PixelFormat),
                                      frame->width, frame->height, AVPixelFormat.AV_PIX_FMT_BGR24,
                              SWS_BICUBIC, null, null, null),
                              frame->data, frame->linesize, 0, frame->height, new[] { (byte*)bitmapData.Scan0.ToPointer() },
                                  new[] { bitmapData.Stride });
                              AvUtil.checkprint(succ, "Error Scaling Image When Coverting to String Iimage");
                              bitmap.UnlockBits(bitmapData);

                              // Convert the Bitmap to a base64 encoded string
                              using (var stream = new MemoryStream())
                              {
                                        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                                        image = Convert.ToBase64String(stream.ToArray());
                              }
                              return image;
                    }
          }
}
