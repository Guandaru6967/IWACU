using FFmpeg.AutoGen;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using static FFmpeg.AutoGen.ffmpeg;
// See https://aka.ms/new-console-template for more information


unsafe class Stream
{

	IntPtr inputfmt = (IntPtr)avformat_alloc_context();

	AVStream* inputstream = null;

	AVPacket* inputpacket = av_packet_alloc();
	AVPacket* outputpacket = av_packet_alloc();

	AVFrame* inputframe = av_frame_alloc();

	AVCodecContext* decoderctx = null;
	AVCodec* decoder = null;
	AVCodecContext* encoderctx = null;
	AVCodec* encoder = avcodec_find_encoder(AVCodecID.AV_CODEC_ID_MJPEG);

	//InputOptions 
	IntPtr videoinputopts = IntPtr.Zero;
	IntPtr audioinputopts = IntPtr.Zero;
	//Input Formats
	IntPtr audioinputfmt = IntPtr.Zero;
	IntPtr videoinputfmt = IntPtr.Zero;
	//Output Format
	IntPtr outputfmt = IntPtr.Zero;

	//Streams

	//Input
	IntPtr audioinputstream = IntPtr.Zero;
	IntPtr videoinputstream = IntPtr.Zero;
	//Output
	IntPtr audiooutputstream = IntPtr.Zero;
	IntPtr videooutputstream = IntPtr.Zero;

	//Codecs
	//Contexts
	AVCodecContext* audiodecoderctx = null;
	AVCodecContext* videodecoderctx = null;

	AVCodecContext* videoencoderctx = null;
	AVCodecContext* audioencoderctx = null;

	//Decoders
	IntPtr audiodecoder = IntPtr.Zero;
	IntPtr videodecoder = IntPtr.Zero;


	//Encoders
	IntPtr audioencoder = IntPtr.Zero;
	IntPtr videoencoder = IntPtr.Zero;

	//Filtergraphs

	//Contexts
	//Video
	IntPtr buffersrcctx = IntPtr.Zero;
	IntPtr buffersnkctx = IntPtr.Zero;
	IntPtr reformatctx = IntPtr.Zero;


	//Audio 

	IntPtr abuffersrcctx = IntPtr.Zero;
	IntPtr abuffersnkctx = IntPtr.Zero;
	IntPtr resamplectx = IntPtr.Zero;

	//Filters

	//Video
	//FilterGraph
	AVFilterGraph* filtergraph = avfilter_graph_alloc();
	//Filters
	IntPtr buffersrcfilter = (IntPtr)avfilter_get_by_name("buffer");
	IntPtr buffersnkfilter = (IntPtr)avfilter_get_by_name("buffersink");
	IntPtr reformatfilter = (IntPtr)avfilter_get_by_name("format");
	//Audio
	//FilterGraph
	AVFilterGraph* afiltergraph = avfilter_graph_alloc();
	//Filters
	IntPtr abuffersrcfilter = (IntPtr)avfilter_get_by_name("abuffer");
	IntPtr abuffersnkfilter = (IntPtr)avfilter_get_by_name("abuffersink");
	AVFilter* resamplefilter = avfilter_get_by_name("aresample");
	//Pakcets

	//IntPtr inputpacket = IntPtr.Zero;

	IntPtr audioinputpacket = (IntPtr)av_packet_alloc();
	IntPtr videoinputpacket = (IntPtr)av_packet_alloc();

	IntPtr audiooutputpacket = (IntPtr)av_packet_alloc();
	IntPtr videooutputpacket = (IntPtr)av_packet_alloc();

	//Frames
	IntPtr audioinputframe = (IntPtr)av_frame_alloc();
	IntPtr videoinputframe = (IntPtr)av_frame_alloc();

	IntPtr audiooutputframe = (IntPtr)av_frame_alloc();
	IntPtr videooutputframe = (IntPtr)av_frame_alloc();

	//Dictionary Options
	IntPtr videoencoderoptions = IntPtr.Zero;

	//Device Variables
	IntPtr deviceInfo = IntPtr.Zero;

	IntPtr audiodevicelist = IntPtr.Zero;
	IntPtr videodevicelist = IntPtr.Zero;


	//Stream Attributes

	int videofps = 30;
	int samplerate = 44100;
	(int, int) video_resolution = (1280, 720);
	string webcaminput = "video=HP Wide Vision HD Camera";
	string microphoneaudioinput = "audio=Microphone Array (Realtek(R) Audio)";
	string headsetaudioinput = "audio=Headset Microphone (Realtek(R) Audio)";



	int succ = 0;


	void checkprint(int error, string message = "Error ")
	{
		if (error < 0)
		{
			Console.WriteLine(message + $" : {printaverror(error)}");

		}

	}
	static unsafe string printaverror(int averrorcode)
	{
		byte* text = (byte*)av_malloc(AV_ERROR_MAX_STRING_SIZE);
		byte[] array = new byte[AV_ERROR_MAX_STRING_SIZE];
		av_make_error_string(text, AV_ERROR_MAX_STRING_SIZE, averrorcode);
		Marshal.Copy((IntPtr)text, array, 0, AV_ERROR_MAX_STRING_SIZE);
		return System.Text.Encoding.UTF8.GetString(array);
	}
	public void SetupDecoders()
	{

	}
	/*void SetupEndoders() 
    {
        //Audio
        audioencoder = (IntPtr)avcodec_find_encoder(AVCodecID.AV_CODEC_ID_MP3);
        audioencoderctx = avcodec_alloc_context3((AVCodec*)audioencoder);
        audioencoderctx->bit_rate = 12800;
        audioencoderctx->sample_fmt = AVSampleFormat.AV_SAMPLE_FMT_FLTP;
        audioencoderctx->channels = 2;
        audioencoderctx->codec_type = AVMediaType.AVMEDIA_TYPE_AUDIO;
        audioencoderctx->sample_rate = 44100;
        audioencoderctx->time_base = new AVRational { num = 1, den = audioencoderctx->sample_rate };

        //Video

        videoencoder = (IntPtr)avcodec_find_encoder(AVCodecID.AV_CODEC_ID_H264);
        videoencoderctx = avcodec_alloc_context3((AVCodec*)videoencoder);
        videoencoderctx->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
        videoencoderctx->width = video_resolution.Item1;
        videoencoderctx->height = video_resolution.Item2;

        videoencoderctx->framerate = new AVRational { num = videofps, den = 1 };
        videoencoderctx->max_b_frames = 3;
        videoencoderctx->gop_size = 10;

        av_dict_set((AVDictionary**)videoencoderoptions, "preset", "medium", 0);
        av_dict_set((AVDictionary**)videoencoderoptions, "crf", "18", 0);
        av_dict_set((AVDictionary**)videoencoderoptions, "preset", "high", 0);
        av_dict_set((AVDictionary**)videoencoderoptions, "tune", "zerolatency", 0);
        av_dict_set((AVDictionary**)videoencoderoptions, "hwaacel", "opencl", AV_DICT_APPEND);
        av_dict_set((AVDictionary**)videoencoderoptions, "opencl", "true", AV_DICT_APPEND);

        if (((AVFormatContext*)outputfmt)->oformat->flags == AVFMT_GLOBALHEADER)
        {
            videoencoderctx->flags |= AV_CODEC_FLAG_GLOBAL_HEADER;
        }
        succ = avcodec_open2(videoencoderctx, (AVCodec*)videoencoder, (AVDictionary**)videoencoderoptions);
        checkprint(succ, "Error Opening Video Encoder");
    }
    */
	void InitFilters()
	{
	}
	void SetupInputStreams()
	{
	}
	void SetupOutputStreams()
	{
	}
	public void recordaudio2(string filename = "recording.mp4", string devicename = "Headset Microphone (Realtek(R) Audio)")
	{
		Console.WriteLine(av_gettime());
		long start = av_gettime();
		/*foreach (var item in Enumerable.Range(0, 100))
        {
            Console.WriteLine(av_rescale_q(av_gettime()+start, new AVRational { num = 1, den = 900000 }, new AVRational { num = 1, den = 1 }));
        }
        return;*/
		devicename = "Microphone Array (Realtek(R) Audio)";
		AVFormatContext* ifmtctx = avformat_alloc_context();
		AVFormatContext* oftmctx = avformat_alloc_context();
		AVFormatContext* audioinput = avformat_alloc_context();

		AVSampleFormat srcformat;
		AVSampleFormat dstformat;
		AVStream* outputstream = null;
		AVStream* videoinputstream = null;
		AVStream* videooutputstream = null;
		AVFrame* videoinputframe = av_frame_alloc();
		AVFrame* videooutputframe = av_frame_alloc();
		AVCodecContext* videoencoder = null;
		AVCodecContext* videodecoder = null;
		AVDictionary* videoopts = null;

		AVCodec* videoencode = avcodec_find_encoder(AVCodecID.AV_CODEC_ID_H264);

		AVCodec* videodecode = null;
		AVFilter* videobuffersrc = avfilter_get_by_name("buffer");

		AVFilter* videobuffersnk = avfilter_get_by_name("buffersink");
		AVFilter* reformat = avfilter_get_by_name("format");
		AVFilterContext* videobuffersrcctx = null;
		AVFilterContext* videobuffersnkctx = null;


		AVFilterContext* reformatctx = null;
		AVPacket* videoinputpacket = av_packet_alloc();
		AVPacket* videooutputpacket = av_packet_alloc();

		AVFilterGraph* videofiltergraph = avfilter_graph_alloc();


		AVFrame* frame = av_frame_alloc();
		AVCodecContext* decoder = null;
		AVDictionary* decodeoptions = null;

		AVCodecContext* encoder = null;

		AVDictionary* encodeoptions = null;

		int videostreamindex = 0;
		int audiostreamindex = 0;
		byte* audiodata = null;
		ushort audiodatasize = 0;
		byte** dstdata = null;
		byte** srcdata = null;
		int dstlinesize = 0;
		int srclinesize = 0;
		avdevice_register_all();
		AVInputFormat* inputformat = null;
		string filepath = $"C:\\Users\\USER\\Desktop\\Development Folder\\Machine Learning\\learning\\StreamApp\\Video-Samples\\{filename}";

		int succ = avformat_alloc_output_context2(&oftmctx, null, "mp4", filepath);// "sdl", "video");
		checkprint(succ, "Error allocating output format");
		inputformat = av_find_input_format("dshow");


		AVDictionary* options = null;
		AVDeviceInfoList* devicelist = null;
		AVCodecParserContext* codecparser = null;

		checkprint(succ, "Unable to get devices list");
		AVDeviceInfo* deviceInfo = null;

		byte* device = null;
		byte[] name = null;
		//string testfile = @"C:\Users\USER\Desktop\Ben Music\aisha_badru_bridges_lyrics_aac_36637.m4a";
		// av_dict_set(&options, "hwaccel", "true", 0);
		av_dict_set(&options, "thread_queue_size", "9999", 0);
		av_dict_set(&options, "rtbufsize", "2147M", 0);
		succ = avformat_open_input(&audioinput, $"audio={devicename}", inputformat, &options);
		checkprint(succ, "Error Opening Audio Device");
		//CUSTOM VIDEO INPUT
		string videodevice = "HP Wide Vision HD Camera";
		AVDictionary* videofmtopt = null;

		int fps = 15;
		av_dict_set(&videofmtopt, "hwaccel", "true", 0);
		//av_dict_set(&videofmtopt, "hwaccel", "true", 0);
		av_dict_set(&videofmtopt, "r", fps.ToString(), 0);
		av_dict_set(&videofmtopt, "video_size", "1280x720", 0);
		// av_dict_set(&videofmtopt, "thread_queue_size", "9999", 0);
		av_dict_set(&videofmtopt, "rtbufsize", "2147M", 0);

		succ = avformat_open_input(&ifmtctx, $"video={videodevice}", inputformat, &videofmtopt);
		checkprint(succ, "Error Opening Video Device");
		succ = avio_open2(&ifmtctx->pb, filepath, AVIO_FLAG_READ, null, &videofmtopt);
		checkprint(succ, "Error Video  Format");
		AVDictionary* dictionary = null;
		/*av_dict_set(&dictionary, "window_size", "1280x720", 0);
        av_dict_set(&dictionary, "window_title", "Main Video", 0);*/

		succ = avio_open2(&oftmctx->pb, filepath, AVIO_FLAG_READ_WRITE, null, &dictionary);
		checkprint(succ, "Error Opening Format");
		succ = avformat_find_stream_info(ifmtctx, null);
		checkprint(succ, "Error Reading Format Context");

		/*succ = avformat_find_stream_info(audioinput, null);
        checkprint(succ, "Error Finding Audio Format Info");*/

		av_dump_format(audioinput, 0, $"audio={devicename}", 0);
		av_dump_format(ifmtctx, 0, $"video={videodevice}", 0);

		//succ = avformat_open_input(&ifmtctx, testfile, null, &options);



		//av_dump_format(ifmtctx,0,testfile,0);
		succ = avdevice_list_devices(ifmtctx, &devicelist);
		/*for (int i = 0; i < devicelist->nb_devices; i++)
       {
           device =   deviceInfo->device_description;
           byte[] array = new byte[AV_ERROR_MAX_STRING_SIZE];
           //av_make_error_string(device, AV_ERROR_MAX_STRING_SIZE, averrorcode);
           Marshal.Copy((IntPtr)device, array, 0, AV_ERROR_MAX_STRING_SIZE);
           Console.WriteLine(System.Text.Encoding.UTF8.GetString(array));
      
       }*/
		SwrContext* resamplectx = swr_alloc();
		AVFrame* inputframe = null;
		AVPacket* inputpacket = av_packet_alloc();
		AVPacket* outputpacket = av_packet_alloc();
		double* inputframbuffer = null;

		//Console.WriteLine(ifmtctx->streams[0]->codecpar->codec_id);
		AVCodec* decode = avcodec_find_decoder(audioinput->streams[0]->codecpar->codec_id);
		//codecparser=av_stream_get_parser(ifmtctx->streams[0]);
		Console.WriteLine(decode->type);
		Console.WriteLine(decode->id);
		// Console.WriteLine(av_codec_is_decoder(decode));

		foreach (int streamcount in Enumerable.Range(0, (int)ifmtctx->nb_streams))
		{
			if (ifmtctx->streams[streamcount]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
			{
				videoinputstream = ifmtctx->streams[streamcount];
				videodecode = avcodec_find_decoder(videoinputstream->codecpar->codec_id);
				videodecoder = avcodec_alloc_context3(videodecode);
				succ = avcodec_parameters_to_context(videodecoder, videoinputstream->codecpar);
				checkprint(succ, "Failed To Copy Video Decoder Params From Stream");
				succ = avcodec_open2(videodecoder, videodecode, null);
				videodecoder->time_base = videoinputstream->time_base;
				checkprint(succ, "Error Opening Video Decoder");
				videostreamindex = streamcount;
			}
			else
			{
				audiostreamindex = streamcount;
			}
		}
		//Console.WriteLine((byte[])decode->long_name);
		Console.WriteLine(decode->type);




		decoder = avcodec_alloc_context3(decode);
		avcodec_parameters_to_context(decoder, audioinput->streams[0]->codecpar);
		//avformat_query_codec()
		decoder->time_base = audioinput->streams[0]->time_base;
		// Console.WriteLine(" Frame size of input codec{0}",ifmtctx->streams[0]->codecpar->frame_size);

		//decoder->frame_size = 22050;
		succ = avcodec_open2(decoder, decode, &decodeoptions);
		checkprint(succ, "Error Opening Decoder");

		checkprint(succ, "Error Opening Output Container");

		/*codecparser = av_parser_init((int)decoder->codec->id);

        if (codecparser == null)
        {
            Console.WriteLine("Parser not Initialized");
        }*/

		AVCodec* encode = avcodec_find_encoder(AVCodecID.AV_CODEC_ID_MP3);

		encoder = avcodec_alloc_context3(encode);
		AVSampleFormat* samples = encode->sample_fmts;

		//Console.WriteLine(av_get_sample_fmt_name(*samples));
		samples++;

		//avcodec_flush_buffers(decoder);
		Console.WriteLine(av_get_sample_fmt_name(*samples));



		AVPixelFormat pixfmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
		videoencoder = avcodec_alloc_context3(videoencode);
		videoencoder->pix_fmt = pixfmt;//AVPixelFormat.AV_PIX_FMT_YUV420P;
		videoencoder->width = videodecoder->width;
		videoencoder->height = videodecoder->height;
		//videoencoder->max_b_frames = 4;
		videoencoder->bit_rate = 4000000;
		//videoencoder->pkt_timebase=videoinputstream->time_base;
		videoencoder->gop_size = 8;

		videoencoder->framerate = new AVRational() { num = videoinputstream->r_frame_rate.num, den = videoinputstream->r_frame_rate.den };
		videoencoder->time_base = new AVRational() { num = 1, den = fps };
		string privdata = "opencl=true";
		if (oftmctx->flags == AVFMT_GLOBALHEADER)
		{
			videoencoder->flags |= AV_CODEC_FLAG_GLOBAL_HEADER;
		}


		Console.WriteLine("3333");
		av_dict_set(&videoopts, "crf", "17", 0);
		//av_dict_set(&videoopts, "tune", "zerolatency", 0);
		//av_dict_set(&videoopts, "profile", "high", 0);
		av_dict_set(&videoopts, "threads", "2", 0);
		av_opt_set(videoencoder->priv_data, "x264-params", privdata, 0);
		succ = avcodec_open2(videoencoder, videoencode, &videoopts);
		checkprint(succ, "Error Opening Video Encoder");

		videooutputstream = avformat_new_stream(oftmctx, videoencode);
		succ = avcodec_parameters_from_context(videooutputstream->codecpar, videoencoder);
		Console.WriteLine((videoinputstream->avg_frame_rate.num, videoinputstream->avg_frame_rate.den));
		Console.WriteLine((videoinputstream->time_base.num, videoinputstream->time_base.den));
		Console.WriteLine((videoinputstream->r_frame_rate.num, videoinputstream->r_frame_rate.den));

		Console.WriteLine((videoinputstream->nb_frames, videoinputstream->start_time, videoinputstream->duration));
		Console.WriteLine((audioinput->streams[0]->nb_frames, audioinput->streams[0]->start_time, audioinput->streams[0]->duration));



		checkprint(succ, "Error Copying Stream Params from Video Encoder");
		// videooutputstream->time_base = videoinputstream->time_base;
		/*
         
        /* set options #1#
        av_opt_set_chlayout(resamplectx, "in_chlayout",    &ifmtctx->streams[0]->codecpar->ch_layout, 0);
        av_opt_set_int(resamplectx, "in_sample_rate",       ifmtctx->streams[0]->codecpar->sample_rate, 0);
        av_opt_set_sample_fmt(resamplectx, "in_sample_fmt", AVSampleFormat.AV_SAMPLE_FMT_S16, 0);
        */


		encoder->sample_fmt = AVSampleFormat.AV_SAMPLE_FMT_FLTP;
		//encoder->pkt_timebase = new AVRational(){num=1,den=41400};
		encoder->sample_rate = 44100;
		//encode->channel_layouts;
		var chlyt = new AVChannelLayout();
		succ = av_channel_layout_from_string(&chlyt, "stereo");
		checkprint(succ, "Not initialized layout");
		encoder->ch_layout = chlyt;
		encoder->bit_rate = 128000;
		//encoder->time_base = new AVRational() {num = 1, den = encoder->sample_rate};
		Console.WriteLine("Frame Size: {0}", encoder->frame_size);
		//return;
		// encoder->frame_size = ifmtctx->streams[0]->codecpar->frame_size;
		//Console.WriteLine("Frame Size: {0}",encoder->frame_size=22050);
		//encoder->ch_layout.nb_channels = 2;
		succ = avcodec_open2(encoder, encode, &encodeoptions);
		checkprint(succ, "Error Opening Encoder");



		string videobuffersrcargs = $"video_size={videodecoder->width}x{videodecoder->height}:pix_fmt={(int)videodecoder->pix_fmt}:frame_rate={videoinputstream->avg_frame_rate.num / videoinputstream->avg_frame_rate.den}:time_base={videoinputstream->time_base.num}/{videoinputstream->time_base.den}:pixel_aspect={videoinputstream->sample_aspect_ratio.num}/{videoinputstream->sample_aspect_ratio.den}";
		string formatargs = $"pix_fmts={av_get_pix_fmt_name(pixfmt)}";

		//CREATE VIDEO FILTERS
		succ = avfilter_graph_create_filter(&videobuffersrcctx, videobuffersrc, "video_in", videobuffersrcargs, null, videofiltergraph);
		checkprint(succ, "Error Creating Video Buffer Source");
		succ = avfilter_graph_create_filter(&reformatctx, reformat, "video_fmt", formatargs, null, videofiltergraph);
		checkprint(succ, "Error Creating Video ReFormat ");
		succ = avfilter_graph_create_filter(&videobuffersnkctx, videobuffersnk, "video_out", "", null, videofiltergraph);
		checkprint(succ, "Error Creating Video Buffer Sink");
		succ = avfilter_link(videobuffersrcctx, 0, reformatctx, 0);
		checkprint(succ, "Error Linking Video Src to Reformat");

		succ = avfilter_link(reformatctx, 0, videobuffersnkctx, 0);
		checkprint(succ, "Error Linking VIdeo Src to Reformat");

		succ = avfilter_graph_config(videofiltergraph, null);
		checkprint(succ, "Error Configuring Video Filter Graph");






		//SET RESAMPLER TO ENCODER
		/*av_opt_set_chlayout(resamplectx, "out_ch_layout",    &decoder->ch_layout, 0);
        av_opt_set_int(resamplectx, "out_sample_rate",       decoder->sample_rate, 0);
        av_opt_set_sample_fmt(resamplectx, "out_sample_fmt", decoder->sample_fmt, 0);*/

		//INIT RESAMPLER
		succ = swr_alloc_set_opts2(&resamplectx,
		  &encoder->ch_layout,
		  encoder->sample_fmt,
		  encoder->sample_rate,
		  &decoder->ch_layout,
		  decoder->sample_fmt,
		  decoder->sample_rate,
		  0, null);
		checkprint(succ, "Error Setting Resampler options");

		//OPEN RESAMPLING CONTEXT
		succ = swr_init(resamplectx);
		checkprint(succ, "Error Initializing Resampling Context");

		//ALLOCATE STREAM FOR OUTPUT FORMAT USING CODEC PARAMETERS
		outputstream = avformat_new_stream(oftmctx, encode);
		succ = avcodec_parameters_from_context(outputstream->codecpar, encoder);
		checkprint(succ, "Errorr Copying Audio Encoder Params to Stream");
		outputstream->codecpar->codec_tag = 0;
		//OPEN OUTPUT FORMAT
		succ = avio_open(&oftmctx->pb, filepath, AVIO_FLAG_WRITE);

		//DISPLAY FORMAT
		av_dump_format(oftmctx, 0, filename, 1);
		encoder->flags |= AV_CODEC_FLAG_GLOBAL_HEADER;


		/*long max_dst_nb_samples = av_rescale_rnd(decoder->max_samples, ifmtctx->streams[0]->codecpar->sample_rate,  ifmtctx->streams[0]->codecpar->sample_rate, AVRounding.AV_ROUND_UP);*/

		if (encoder->sample_rate == decoder->sample_rate)
		{
			Console.WriteLine("Encoder sample rate -> {0}\n Decoder sample rate -> {1}", encoder->sample_rate, decoder->sample_rate);
		}

		/* buffer is going to be directly written to a rawaudio file, no alignment */
		// dst_nb_channels = dst_ch_layout.nb_channels;
		checkprint(succ, "Error Initializing  Audio Resampler");
		Console.WriteLine("-----------SAMPLE DATA--------");
		Console.WriteLine(encoder->bits_per_raw_sample);
		Console.WriteLine(encoder->max_samples);
		Console.WriteLine(encoder->bits_per_coded_sample);
		/*succ = av_samples_alloc_array_and_samples(&dstdata, &dstlinesize, encoder->ch_layout.nb_channels, (int)max_dst_nb_samples,
            encoder->sample_fmt, 0);
        dstdata = (byte**)av_calloc((ulong)encoder->ch_layout.nb_channels,(ulong)sizeof(byte*));
        var num=sizeof (AVFormatContext*);*/



		/*audiodata = (byte*)av_malloc((AV_INPUT_BUFFER_MIN_SIZE+AV_INPUT_BUFFER_PADDING_SIZE));
        audiodatasize = AV_INPUT_BUFFER_MIN_SIZE;
        List< AVPacket> packets=new List< AVPacket>();*/
		// Console.WriteLine("Recording Audio");

		/*{
     
            av_read_frame(ifmtctx, inputpacket);
            packets.Add(*inputpacket);
            av_packet_unref(inputpacket);
        }*/
		//Console.WriteLine("Packets Collected ");

		// avformat_close_input(&ifmtctx);
		// videooutputstream->time_base = videoinputstream->time_base;
		// outputstream->time_base = audioinput->streams[0]->time_base;
		Console.WriteLine(videooutputstream->time_base.den);
		Console.WriteLine(outputstream->time_base.den);
		succ = avformat_write_header(oftmctx, null);
		checkprint(succ, "Failed to write header");
		Console.WriteLine(videooutputstream->time_base.den);
		Console.WriteLine(outputstream->time_base.den);
		//return;

		AVFrame* outframe = av_frame_alloc();
		int samples_count = 0;
		long audiopts = 0;

		//outputstream->time_base = new AVRational() {num =1 , den = 10000000};

		Console.WriteLine("decoder->time_base : {0}", decoder->time_base.den);
		Console.WriteLine("encoder->time_base : {0}", encoder->time_base.den);
		Console.WriteLine($"in_chlayout=stereo:in_sample_fmt={(int)audioinput->streams[0]->codecpar->format}:in_sample_rate={audioinput->streams[0]->codecpar->sample_rate}:out_sample_fmt={outputstream->codecpar->format}:out_sample_rate={outputstream->codecpar->sample_rate}:out_chlayout=stereo");
		Console.WriteLine("inpust stream->time_base : {0}", audioinput->streams[0]->time_base.den);
		Console.WriteLine("output stream ->time_base : {0}", outputstream->time_base.den);
		Console.WriteLine("encoder ->frame_size : {0}", encoder->frame_size);

		Console.WriteLine("video decoder->time_base : {0}", videodecoder->time_base.den);
		Console.WriteLine("video encoder->time_base : {0}", videoencoder->time_base.den);

		Console.WriteLine("video input stream->time_base : {0}", videoinputstream->time_base.den);
		Console.WriteLine("video output stream ->time_base : {0}", videooutputstream->time_base.den);

		int audiocount = 0;
		int videocount = 0;

		videostreamindex = 0;
		audiostreamindex = 1;

		/*for (int counts = 0; counts < 30; counts++)
        {
            AVPacket* acollector = av_packet_alloc();
            av_read_frame(audioinput, acollector);
            av_packet_unref(acollector);
        }*/


		long lastpts = 0;
		Thread.Sleep(1000 * 1);
		Console.WriteLine("Video Start Time Before Flush  : {0}", ifmtctx->streams[0]->start_time);
		//succ = avformat_flush(audioinput);
		//checkprint(succ, "Error Flushing Audio Input Fmt");
		/*succ = avformat_flush(ifmtctx);

		checkprint(succ, "Error Flushing Video Input Fmt");*/


		Console.WriteLine("Video Start Time After Flush  : {0}", ifmtctx->streams[0]->start_time);
		Console.WriteLine(videoinputstream->start_time);
		Console.WriteLine(audioinput->streams[0]->start_time);
		Console.WriteLine(av_rescale_q(audioinput->streams[0]->start_time, audioinput->streams[0]->time_base, outputstream->time_base));
		Console.WriteLine(av_rescale_q(videoinputstream->start_time, videoinputstream->time_base, videooutputstream->time_base));
		Console.WriteLine(videoinputstream->start_time - audioinput->streams[0]->start_time);
		Console.WriteLine(av_rescale_q(videoinputstream->start_time - audioinput->streams[0]->start_time, audioinput->streams[0]->time_base, outputstream->time_base));
		long videodelay = av_rescale_q(videoinputstream->start_time - audioinput->streams[0]->start_time, audioinput->streams[0]->time_base, outputstream->time_base);
		AVPacket* packet = av_packet_alloc();
		AVPacket* impacket = av_packet_alloc();
		long video_starttime = av_rescale_q(ifmtctx->streams[0]->start_time, ifmtctx->streams[0]->time_base, videooutputstream->time_base);
		long audio_starttime = av_rescale_q(audioinput->streams[0]->start_time, audioinput->streams[0]->time_base, outputstream->time_base);
		long delta = video_starttime - audio_starttime;
		long video_pts;
		long audio_pts;

		long video_dts;
		long audio_dts;

		videooutputstream->start_time =video_starttime;
		//outputstream->start_time = audio_starttime;
		Console.WriteLine("{0} Started Fast ", video_starttime < audio_starttime ? "Video" : "Audio");
		Console.WriteLine(((ulong )audio_starttime, video_starttime));
		//videooutputstream->time_base=
		Console.WriteLine($"{videoencoder->framerate.num / videoencoder->framerate.den} is the time");
		int duration = 10;
		/*	succ = av_read_frame(audioinput, inputpacket);
			checkprint(succ, "Error Reading Frame");
			long targetpts = inputpacket->pts;
			long ptstarget = 0;
			succ = av_read_frame(ifmtctx, packet);
			checkprint(succ, "Error Reading Frame");
			if (targetpts != ptstarget) 
			{
				Console.WriteLine($"Not Reached pts {targetpts} latest pts was at {ptstarget}");
			Console.WriteLine($"Diff in packet size is: {Convert.ToDouble((packet->pts- inputpacket->pts)/ Convert.ToDouble(videoinputstream->time_base.den)) }");
			}
			else
			{
				Console.WriteLine($"Reached pts {targetpts}");
			}
			*/
		
		//return;
			for (int count = 1; count < 30 * duration; count++)
		{
			succ = av_channel_layout_copy(&outframe->ch_layout, &encoder->ch_layout);
			checkprint(succ, "Failed to copy channel layout");
			/*Console.WriteLine(encoder->pkt_timebase.num/encoder->pkt_timebase.den);
            Console.WriteLine(encoder->time_base.num/encoder->time_base.den);
            break;*/
			outframe->sample_rate = encoder->sample_rate;
			outframe->nb_samples = encoder->frame_size;
			//encoder->frame_size = decoder->frame_size;
			outframe->format = (int)encoder->sample_fmt;
			// Console.WriteLine("Buffer Size {0}", av_frame_unref();
			//  Console.WriteLine(audioinput == null);
			succ = av_read_frame(audioinput, inputpacket);
			checkprint(succ, "Error Reading Frame");

			Console.WriteLine("Audio Stream Start Time  : {0}", audioinput->streams[0]->start_time);
			Console.WriteLine("Audio Stream Duration  : {0}", audioinput->streams[0]->duration);
			Console.WriteLine("Audio Rescaled Packet PTS : {0} | Destination TimeBase {1} /{2}", av_rescale_q(inputpacket->pts, audioinput->streams[0]->time_base, outputstream->time_base), outputstream->time_base.num, outputstream->time_base.den);


			Console.WriteLine("Audio Packet PTS : {0}", inputpacket->pts);
			Console.WriteLine("Audio Packet DTS : {0}", inputpacket->dts);
			Console.WriteLine("Audio Packet POS : {0}", inputpacket->pos);
			Console.WriteLine("Audio Packet Duration: {0} ", inputpacket->duration);
			Console.WriteLine("Audio Count :{0}", audiocount);
			Console.WriteLine("PTS/TIME BASE : {0}", inputpacket->pts / audioinput->streams[0]->time_base.den);
			Console.WriteLine("Audio Stream Timebase:{0}/{1}", audioinput->streams[0]->time_base.num, audioinput->streams[0]->time_base.den);
			Console.WriteLine("Audio Stream Average Framerate:{0}/{1}", audioinput->streams[0]->avg_frame_rate.num, audioinput->streams[0]->avg_frame_rate.den);
			Console.WriteLine("Audio Stream Sample Rate:{0}", audioinput->streams[0]->codecpar->sample_rate);

			audio_pts = av_rescale_q(inputpacket->pts, audioinput->streams[0]->time_base, outputstream->time_base);

			Console.WriteLine("audio_pts:{0}", audio_pts);

			

			succ = av_channel_layout_copy(&outframe->ch_layout, &encoder->ch_layout);
			checkprint(succ, "Failed to copy channel layout");
			/*Console.WriteLine(encoder->pkt_timebase.num/encoder->pkt_timebase.den);
            Console.WriteLine(encoder->time_base.num/encoder->time_base.den);
            break;*/
			outframe->sample_rate = encoder->sample_rate;
			outframe->nb_samples = encoder->frame_size;
			//encoder->frame_size = decoder->frame_size;
			outframe->format = (int)encoder->sample_fmt;
			// Console.WriteLine("Buffer Size {0}", av_frame_unref();

			/*succ = swr_alloc_set_opts2(&resamplectx,
                &outframe->ch_layout,
                (AVSampleFormat)outframe->format,
                outframe->sample_rate,
                &frame->ch_layout,
                (AVSampleFormat)frame->format,
                frame->sample_rate,
                0, null);   
            checkprint(succ,"Error Setting swr config");*/
			/*Console.WriteLine(inputpacket->duration);
            Console.WriteLine(inputpacket->pts);
            Console.WriteLine(inputpacket->dts);*/

			/*Console.WriteLine("Read Packet");
            Console.WriteLine(inputpacket->dts);
            Console.WriteLine(inputpacket->pts);
            Console.WriteLine(inputpacket->size);
            Console.WriteLine(inputpacket->time_base.den);*/

			/*packet=av_packet_clone(inputpacket);
            av_packet_make_writable(inputpacket);*/
			//
			//avformat_close_input(&ifmtctx);
			/*checkprint(succ,"Error Reading Frame from InputFormat");
            */
			//avcodec_receive_frame(decoder,frame);

			// av_packet_rescale_ts(inputpacket, audioinput->streams[0]->time_base, decoder->time_base);

			succ = avcodec_send_packet(decoder, inputpacket);
			checkprint(succ, "Error Sending Packet to Decoder");
			//AVFrame** frames = null;
			// *frames = av_frame_alloc();
			succ = avcodec_receive_frame(decoder, frame);
			checkprint(succ, "Error Reading Frame from Decoder");

			/*Console.WriteLine("After");
            Console.WriteLine(inputpacket->duration);
            Console.WriteLine(inputpacket->pts);
            Console.WriteLine(inputpacket->dts);
            Console.WriteLine("Frame");
            Console.WriteLine(frame->pkt_duration);
            Console.WriteLine(frame->pts);
            Console.WriteLine(frame->pkt_dts);
            break;*/
			/*Console.WriteLine("---------ENCODER---------");
            Console.WriteLine(encoder->ch_layout.nb_channels);
            Console.WriteLine(encoder->sample_rate);
            Console.WriteLine(encoder->frame_size);
            Console.WriteLine("---------DECODER---------");
            Console.WriteLine(decoder->ch_layout.nb_channels);
            Console.WriteLine(decoder->sample_rate);
            Console.WriteLine(decoder->frame_size);
            Console.WriteLine("--------IN FRAME------- 1");
            Console.WriteLine(frame->ch_layout.nb_channels);
            Console.WriteLine(frame->sample_rate);
            Console.WriteLine(frame->nb_samples);
            Console.WriteLine(frame->pts);
            Console.WriteLine("--------OUT FRAME-------1");
            Console.WriteLine(outframe->ch_layout.nb_channels);
            Console.WriteLine(outframe->sample_rate);
            Console.WriteLine(outframe->pts);
            Console.WriteLine(outframe->nb_samples);*/
			/*succ = swr_config_frame(resamplectx, outframe, frame);

            checkprint(succ,"Error configuring frames");*/
			succ = swr_convert_frame(resamplectx, outframe, frame);


			// av_frame_unref(frame);
			checkprint(succ, "Error Converting frame in Resampling Context");
			outframe->pts = outframe->best_effort_timestamp;

			/*Console.WriteLine("--------IN FRAME-------");
            Console.WriteLine(frame->ch_layout.nb_channels);
            Console.WriteLine(frame->sample_rate);
            Console.WriteLine(frame->nb_samples);
            Console.WriteLine(frame->pts);
            Console.WriteLine("--------OUT FRAME-------");
            Console.WriteLine(outframe->ch_layout.nb_channels);
            Console.WriteLine(outframe->sample_rate);
            Console.WriteLine(outframe->nb_samples);
            Console.WriteLine(outframe->pts);
            Console.WriteLine(encoder->frame_size);*/
			//frame->pts = av_rescale_q(samples_count, new AVRational{den = 1, num=encoder->sample_rate}, encoder->time_base);

			/*for (int num = 0; num > 2; num++)
            {

            }*/

			//avcodec_get_name();
			succ = avcodec_send_frame(encoder, outframe);

			checkprint(succ, "Error Sending Frame to Encoder");
			succ = avcodec_receive_packet(encoder, outputpacket);

			checkprint(succ, "Error Receiving Packet from Audio Encoder");
			if (succ < 0)
			{
				continue;
			}

			/*outputpacket->pts = inputpacket->pts;
            outputpacket->dts = inputpacket->dts;
            av_packet_rescale_ts(outputpacket, encoder->time_base, outputstream->time_base);
            */
			if (videodelay <= encoder->frame_size * audiocount)
			{
				//outputpacket->duration = encoder->frame_size;
				/*outputpacket->pts = encoder->frame_size * audiocount;
                outputpacket->dts = outputpacket->pts;
                outputpacket->dts = audio_pts;
                outputpacket->pts = audio_pts;*/
				av_packet_rescale_ts(outputpacket, decoder->time_base, encoder->time_base);

				long dts = av_rescale_q(av_gettime(), oftmctx->streams[audiostreamindex]->time_base, new AVRational { num = 1, den = 1 });
				outputpacket->pts = audiocount != 0 ? outputpacket->pts : 0;
				outputpacket->dts = audiocount != 0 ? outputpacket->pts : 0;

				//TEST PTS 
				/*
                outputpacket->dts = dts;
                outputpacket->pts = dts;
                 */
				//outputpacket->duration = 1152;
				//outputpacket->duration = encoder->frame_size;
				/*  outputpacket->pts = (1 * audiocount * outputstream->time_base.den /
                                        (outputstream->time_base.num * encoder->sample_rate));
                  outputpacket->dts = outputpacket->pts;

                  outputpacket->dts = outputpacket->pts;*/
				//audiocount++;
				audiocount++;
				outputpacket->stream_index = audiostreamindex;
				long delta_AV = av_rescale_q(video_starttime, videoinputstream->time_base, oftmctx->streams[audiostreamindex]->time_base);
				Console.WriteLine(delta_AV);
				Console.WriteLine((outputpacket->dts, outputpacket->dts + delta_AV));
				if (outputpacket->dts > 0)
				{
					return;
				}
				/*Console.WriteLine("Audio PTS : {0}", outputpacket->pts);
                Console.WriteLine("Audio DTS : {0}", outputpacket->dts);
                Console.WriteLine("Audio : {0}", outputpacket->pos);
                Console.WriteLine(outputpacket->duration);
                Console.WriteLine("Audio Count{0}",audiocount*count);
                Console.WriteLine("PTS/TIME BASE : {0}", outputpacket->pts / outputstream->time_base.den);
                Console.WriteLine("Audio Stream Timebase:{0}",audioinput->streams[0]->time_base);
                Console.WriteLine("Audio Stream Average Framerate:{0}",audioinput->streams[0]->avg_frame_rate);
                Console.WriteLine("Audio Stream Average Framerate:{0}",audioinput->streams[0]->codecpar->sample_rate); 
                */
				succ = av_write_frame(oftmctx, outputpacket);
				Console.WriteLine(100);
				checkprint(succ, "Error Writing Packet to format");
			}
		}

		for (var i = 0; i < (videoencoder->framerate.num / videoencoder->framerate.den) * duration; i++) // foreach(var packet in packets)
		{
			//AVPacket* packet = av_packet_alloc();f

			//Console.WriteLine("Reading Camera");
			succ = av_read_frame(ifmtctx, packet);
			checkprint(succ, "Error Reading Video Frame");
			video_pts = av_rescale_q(packet->pts, ifmtctx->streams[0]->time_base, videooutputstream->time_base) - video_starttime;



			Console.WriteLine("Video Stream Start Time  : {0}", ifmtctx->streams[0]->start_time);
			Console.WriteLine("Video Stream Duration  : {0}", ifmtctx->streams[0]->duration);
			Console.WriteLine("Video Rescaled Packet PTS : {0} | Destination TimeBase {1} /{2}", av_rescale_q(packet->pts, ifmtctx->streams[0]->time_base, videooutputstream->time_base), videooutputstream->time_base.num, videooutputstream->time_base.den);

			Console.WriteLine("Video Packet PTS : {0}", packet->pts);
			Console.WriteLine("Video Packet DTS : {0}", packet->dts);
			Console.WriteLine("Video Packet POS : {0}", packet->pos);
			Console.WriteLine("Video Packet Duration :{0}", packet->duration);
			Console.WriteLine("Video Count : {0}", i);
			Console.WriteLine("VIDEO :\n [ PTS/TIME BASE : {0}] \n [ (PTS/TIME BASE )/FPS : {1} ]", packet->pts / ifmtctx->streams[0]->time_base.den, (packet->pts / ifmtctx->streams[0]->time_base.den) / 15);
			Console.WriteLine("Video Stream Timebase: {0}/{1}", ifmtctx->streams[0]->time_base.num, ifmtctx->streams[0]->time_base.den);
			Console.WriteLine("Video Stream Average Framerate: {0}/{1}", ifmtctx->streams[0]->avg_frame_rate.num, ifmtctx->streams[0]->avg_frame_rate.den);
			Console.WriteLine("Video Stream Average Framerate: {0}x{1}", ifmtctx->streams[0]->codecpar->width, ifmtctx->streams[0]->codecpar->height);
			Console.WriteLine("video_pts:{0}", video_pts);
			
			/*vpts = videoinputpacket->pts;
            vdts = videoinputpacket->dts;
            vdtn = videoinputpacket->duration;*/

			do
			{

				/*Console.WriteLine(videoinputpacket->pts);
                Console.WriteLine(videoinputpacket->dts);
                Console.WriteLine(("fefef"));
           av_packet_rescale_ts(videoinputpacket,videoinputstream->time_base,videodecoder->time_base);
           Console.WriteLine(videoinputpacket->pts);
           Console.WriteLine(videoinputpacket->dts);
           return;*/
				succ = avcodec_send_packet(videodecoder, packet);
				checkprint(succ, "Error Sending Packet to Decoder");
				succ = avcodec_receive_frame(videodecoder, videoinputframe);
				checkprint(succ, "Error Receiving Frame from Decoder");
			}
			while (succ < 0);


			succ = av_buffersrc_add_frame(videobuffersrcctx, videoinputframe);
			checkprint(succ, "Error Adding Frame to Filter Graph");

			succ = av_buffersink_get_frame(videobuffersnkctx, videooutputframe);
			checkprint(succ, "Error Sending Frame From Filter graph");

			/*videooutputframe->pts =  (long)(0.5 * av_gettime() * videooutputstream->time_base.den / (videooutputstream->time_base.num * videoencoder->framerate.num));
            videooutputframe->pkt_dts = videooutputpacket->pts;*/
			//videooutputframe->pts = av_gettime();
			/*videooutputframe->pts = videooutputframe->best_effort_timestamp;
            videooutputframe->pkt_dts = videoinputframe->best_effort_timestamp;*/
			//  videooutputframe->pkt_duration = videoinputframe->pkt_duration;
			//videooutputframe->time_base = videoinputframe->time_base;


			succ = avcodec_send_frame(videoencoder, videooutputframe);
			checkprint(succ, "Error Sending Frame to Encoder");
			succ = avcodec_receive_packet(videoencoder, videooutputpacket);
			checkprint(succ, "Error Receiving Packet from Encoder");

			/*
             av_packet_rescale_ts(videooutputpacket, videoinputstream->time_base, videooutputstream->time_base);*/


			// av_packet_rescale_ts(videooutputpacket, videoencoder->time_base, videooutputstream->time_base);


			videooutputpacket->pts = i == 0 ? 0 : video_pts; // (long)(1 * videocount * videooutputstream->time_base.den /
															 // (videooutputstream->time_base.num * videoencoder->framerate.num / videoencoder->framerate.den));
			/*videooutputpacket->dts = (1 * videocount * videooutputstream->time_base.den /
                                      (videooutputstream->time_base.num * fps));*/
			//-video_starttime;
			videooutputpacket->dts = i == 0 ? 0 : video_pts;
			videocount++;

			/* videooutputpacket->dts = vdts;
             videooutputpacket->pts = vpts;
             videooutputpacket->duration = vdtn;*/
			videooutputpacket->stream_index = videostreamindex;


			//Console.WriteLine("VIDEO DTS:{0} PTS :{1} ", videooutputpacket->dts, videooutputpacket->pts);
			//Console.WriteLine(inputpacket->pts);

			//PTS TEST
			/*
            long dts = av_rescale_q(av_gettime() , oftmctx->streams[videostreamindex]->time_base, new AVRational { num = 1, den = 1 });
            videooutputpacket->dts = dts;
            videooutputpacket->pts = dts;
            Console.WriteLine($" Input Time:{packet->dts/videoinputstream->time_base.den} {packet->duration}");
            */
			//return;

			succ = av_interleaved_write_frame(oftmctx, videooutputpacket);
			checkprint(succ, "Error Writing Interleaved Video Frame");





			av_packet_unref(inputpacket);
			av_packet_unref(outputpacket);

			av_frame_unref(inputframe);
			av_frame_unref(frame);

			//break;
			if (succ < 0)
			{
				continue;

			}
			else
			{
				Console.WriteLine("Received Frame!");
			}

			/*while (succ > 0)
            {

            }*/

			//av_packet_unref(inputpacket);




			int vsucc = 0;
			int readcount = 0;
			while (succ >= 0 && vsucc >= 0)
			{
				succ = avcodec_receive_packet(encoder, outputpacket);
				vsucc = avcodec_receive_packet(videoencoder, videooutputpacket);

				outputpacket->stream_index = audiostreamindex;
				videooutputpacket->stream_index = videostreamindex;
				Console.WriteLine(readcount);
				if (succ >= 0 && vsucc > 0)
				{
					av_write_frame(oftmctx, outputpacket);
					av_interleaved_write_frame(oftmctx, videooutputpacket);
				}
				else
				{
					break;
				}

				readcount++;

				av_packet_unref(outputpacket);
				av_packet_unref(videooutputpacket);
			}


			Console.WriteLine($"Read {readcount} Packets");
			Console.WriteLine("Done Sending Frames to encoder");
		}
		
		av_write_trailer(oftmctx);
		avformat_close_input(&ifmtctx);
		Console.WriteLine(audiocount);
		Console.WriteLine(videocount);
		avio_close(oftmctx->pb);

	}
	public void ZoomAndSpan()
	{

	}
	public void Record(string filename = "C:\\Users\\USER\\Desktop\\Development Folder\\Machine Learning\\learning\\StreamApp\\Video-Samples\\output.mp4", string audiodevicename = "Microphone Array (Realtek(R) Audio)", string webcamname = "HP Wide Vision HD Camera")
	{
		//Init Devices
		avdevice_register_all();
		 resamplefilter = avfilter_get_by_name("aresample");

		//Allocate Formats
		//Input Format
		videoinputfmt = (IntPtr)avformat_alloc_context();
		audioinputfmt = (IntPtr)avformat_alloc_context();

		//Set Format Parameters

		//Video
		//av_dict_set((AVDictionary**)videoinputopts, "hwaccel", "true", 0);
		//av_dict_set(&videofmtopt, "hwaccel", "true", 0);
		AVDictionary* vopts = (AVDictionary*)videoinputopts.ToPointer();
		av_dict_set(&vopts, "r", videofps.ToString(), 0);
		av_dict_set(&vopts, "video_size", $"{video_resolution.Item1}x{video_resolution.Item2}", 0);
		// av_dict_set( &vopts, "thread_queue_size", "9999", 0);
		av_dict_set(&vopts, "rtbufsize", "2147M", 0);


		//Video
		((AVFormatContext*)videoinputfmt)->iformat = av_find_input_format("dshow");
		AVFormatContext* vinputfmt = (AVFormatContext*)videoinputfmt;
		succ = avformat_open_input((AVFormatContext**)&vinputfmt, webcaminput, vinputfmt->iformat, &vopts);
		checkprint(succ, "Error Opening Web Cam input");


		//Audio
		AVDictionary* audioinput_options = (AVDictionary*)audioinputopts;
		av_dict_set(&audioinput_options, "thread_queue_size", "9999", 0);
		av_dict_set(&audioinput_options, "rtbufsize", "2147M", 0);

		((AVFormatContext*)audioinputfmt)->iformat = av_find_input_format("dshow");
		AVFormatContext* audioinput_fmt = (AVFormatContext*)audioinputfmt;
		succ = avformat_open_input((AVFormatContext**)&audioinput_fmt, microphoneaudioinput, audioinput_fmt->iformat, &audioinput_options);
		checkprint(succ, "Error Opening Audio input");

		//Get Input Format Info
		succ = avformat_find_stream_info((AVFormatContext*)videoinputfmt, null);
		checkprint(succ, "Error getting Video Stream Info");
		succ = avformat_find_stream_info((AVFormatContext*)audioinputfmt, null);
		checkprint(succ, "Error getting Audio Stream Info");
		//Get Devices
		//Video
		AVDeviceInfoList* vdevlist = (AVDeviceInfoList*)videodevicelist;
		int videodevicecount = avdevice_list_devices((AVFormatContext*)videoinputfmt, &vdevlist);
		checkprint(videodevicecount, "Error Unable to Get Video Device List");

		Console.WriteLine("Found {0} Video Devices", videodevicecount);
		//Audio
		AVDeviceInfoList* adevlist = (AVDeviceInfoList*)audiodevicelist;
		int audiodevicecount = avdevice_list_devices((AVFormatContext*)audioinputfmt, &adevlist);
		checkprint(audiodevicecount, "Error Unable to Get Audio Device List");

		Console.WriteLine("Found {0} Audio Devices", audiodevicecount);

		//Output Format
		outputfmt = (IntPtr)avformat_alloc_context();
		AVFormatContext* outputcontext = (AVFormatContext*)outputfmt;

		succ = avformat_alloc_output_context2(&outputcontext, null, "mp4", filename);
		checkprint(succ, "Error opening Output Format Context");

		//Show Formatr info 
		av_dump_format((AVFormatContext*)audioinputfmt, 0, microphoneaudioinput, 0);
		av_dump_format((AVFormatContext*)videoinputfmt, 0, webcamname, 0);

		//Find Streams and Decoders
		//Video
		foreach (int count in Enumerable.Range(0, (int)((AVFormatContext*)videoinputfmt)->nb_streams))
		{
			if (((AVFormatContext*)videoinputfmt)->streams[count]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
			{
				videoinputstream = (IntPtr)((AVFormatContext*)videoinputfmt)->streams[count];
				videodecoder = (IntPtr)avcodec_find_decoder(((AVStream*)videoinputstream)->codecpar->codec_id);
				videodecoderctx = avcodec_alloc_context3((AVCodec*)videodecoder);

				succ = avcodec_parameters_to_context(videodecoderctx, ((AVStream*)videoinputstream)->codecpar);
				checkprint(succ, "Error Copying  Parameters to Video Codec");
				//Open Decoder
				succ = avcodec_open2(videodecoderctx, (AVCodec*)videodecoder, null);
				checkprint(succ, "Error Opening Video Decoder");




			}
		}

		//Audio
		foreach (int count in Enumerable.Range(0, (int)((AVFormatContext*)audioinputfmt)->nb_streams))
		{
			if (((AVFormatContext*)audioinputfmt)->streams[count]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
			{
			
				audioinputstream = (IntPtr)((AVFormatContext*)audioinputfmt)->streams[count];
				audiodecoder = (IntPtr)avcodec_find_decoder(((AVStream*)audioinputstream)->codecpar->codec_id);
				audiodecoderctx = avcodec_alloc_context3((AVCodec*)audiodecoder);


				//Open Decoder
				audiodecoderctx->time_base = ((AVStream*)audioinputstream)->time_base;
				//audiodecoderctx->frame_size = 22050;

				succ = avcodec_parameters_to_context(audiodecoderctx, ((AVStream*)audioinputstream)->codecpar);
				checkprint(succ, "Error Copying Parameters to Audio Context");

				succ = avcodec_open2(audiodecoderctx, (AVCodec*)audiodecoder, null);
				checkprint(succ, "Error Opening Audio Decoder");




			}
		}
		//Setup Encoders
		//Video
		videoencoder = (IntPtr)avcodec_find_encoder_by_name("libx264");
		videoencoderctx = avcodec_alloc_context3((AVCodec*)videoencoder);

		videoencoderctx->gop_size = 12;

		videoencoderctx->width = video_resolution.Item1;
		videoencoderctx->height = video_resolution.Item2;
		videoencoderctx->framerate = new AVRational { num = videofps, den = 1 };
		videoencoderctx->max_b_frames = 3;
		videoencoderctx->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
		videoencoderctx->time_base = new AVRational { num = 1, den = 90000 };

		if (((AVFormatContext*)outputfmt)->flags == AVFMT_GLOBALHEADER)
		{
			videoencoderctx->flags |= AV_CODEC_FLAG_GLOBAL_HEADER;
		}
		//Set Encoder Parameters
		AVDictionary* vencoderopts = (AVDictionary*)videoencoderoptions;



		av_dict_set(&vencoderopts, "profile", "high", 0);
		av_dict_set(&vencoderopts, "preset", "medium", 0);
		av_dict_set(&vencoderopts, "crf", "18", 0);
		av_dict_set(&vencoderopts, "tune", "zerolatency", 0);
		av_dict_set(&vencoderopts, "opencl", "true", 0);

		succ = av_opt_set(videoencoderctx->priv_data, "x264-params", "opencl=true", 0);
		checkprint(succ, "Error Setting Priv Data to Video encoder");

		succ = avcodec_open2(videoencoderctx, (AVCodec*)videoencoder, &vencoderopts);
		checkprint(succ, "Error Opening Videoencoder");
		//Audio
		audioencoder = (IntPtr)avcodec_find_encoder(AVCodecID.AV_CODEC_ID_AAC);
		audioencoderctx = avcodec_alloc_context3((AVCodec*)audioencoder);
		audioencoderctx->sample_fmt = AVSampleFormat.AV_SAMPLE_FMT_FLTP;// *((AVCodec*)audioencoder)->sample_fmts;
		audioencoderctx->sample_rate = 44100;
		//Create Audio Encoder Channel Layout 
		var chlyt = new AVChannelLayout();
		succ = av_channel_layout_from_string(&chlyt, "stereo");
		checkprint(succ, "Not initialized layout");
		audioencoderctx->ch_layout = chlyt;
		audioencoderctx->bit_rate = 128000;

		succ = avcodec_open2(audioencoderctx, (AVCodec*)audioencoder, null);
		checkprint(succ, "Error Opening Audio encoder");

		//Open Streams

		//Audio
		audiooutputstream = (IntPtr)avformat_new_stream(outputcontext, (AVCodec*)audioencoder);
		succ = avcodec_parameters_from_context(((AVStream*)audiooutputstream)->codecpar, audioencoderctx);
		checkprint(succ, "Error Copying Parameters to Audio Output Stream");

		//Video
		videooutputstream = (IntPtr)avformat_new_stream(outputcontext, (AVCodec*)videoencoder);
		succ = avcodec_parameters_from_context(((AVStream*)videooutputstream)->codecpar, videoencoderctx);
		checkprint(succ, "Error Copying Parameters to Video Output Stream");

		//Setup FIltergraph
		//Video 
		//Filter Args


		string videobuffersrcargs = $"video_size={video_resolution.Item1}x{video_resolution.Item2}:pix_fmt={(int)videodecoderctx->pix_fmt}:frame_rate={((AVStream*)videoinputstream)->avg_frame_rate.num / ((AVStream*)videoinputstream)->avg_frame_rate.den}:time_base={((AVStream*)videoinputstream)->time_base.num}/{((AVStream*)videoinputstream)->time_base.den}:pixel_aspect={((AVStream*)videoinputstream)->sample_aspect_ratio.num}/{((AVStream*)videoinputstream)->sample_aspect_ratio.den}";
		string formatargs = $"pix_fmts={av_get_pix_fmt_name(videoencoderctx->pix_fmt)}";


		/*buffersrcctx = (IntPtr)avfilter_graph_alloc_filter((AVFilterGraph*)filtergraph, (AVFilter*) buffersrcfilter, "vin");
        buffersnkctx = (IntPtr)avfilter_graph_alloc_filter((AVFilterGraph*)filtergraph, (AVFilter*)buffersnkfilter, "vout");
        reformatctx = (IntPtr)avfilter_graph_alloc_filter((AVFilterGraph*)filtergraph, (AVFilter*)reformatfilter, "format");

       */


		AVFilterContext* videobuffersrc_context = (AVFilterContext*)buffersrcctx;
		AVFilterContext* videobuffersnk_context = (AVFilterContext*)buffersnkctx;
		AVFilterContext* videoformat_context = (AVFilterContext*)reformatctx;

		succ = avfilter_graph_create_filter(&videobuffersrc_context, (AVFilter*)buffersrcfilter, "video_in", videobuffersrcargs, null, filtergraph);
		checkprint(succ, "Error Creating BufferSrc Filter");

		succ = avfilter_graph_create_filter(&videoformat_context, (AVFilter*)reformatfilter, "video_format", formatargs, null, filtergraph);
		checkprint(succ, "Error Creating Video Reformat Filter");

		succ = avfilter_graph_create_filter(&videobuffersnk_context, (AVFilter*)buffersnkfilter, "video_out", "", null, filtergraph);
		checkprint(succ, "Error Creating Video Buffer Sink");

		succ = avfilter_link(videobuffersrc_context, 0, videoformat_context, 0);
		checkprint(succ, "Error Linking Video Bufffer Source To Format Filter");

		succ = avfilter_link(videoformat_context, 0, videobuffersnk_context, 0);
		checkprint(succ, "Error Linking  Format Filter to Video Bufffer Sink");

		succ = avfilter_graph_config(filtergraph, null);
		checkprint(succ, "Error Configuring Filtergraph");

	
		//Audio
		string audiobuffersrcargs = $"channel_layout=stereo:sample_rate={(audiodecoderctx)->sample_rate}:sample_fmt={(int)audiodecoderctx->sample_fmt}:channels={((AVStream*)audioinputstream)->codecpar->ch_layout.nb_channels}";
		string resampleargs = $"in_chlayout=stereo:in_sample_fmt={(int)((AVStream*)audioinputstream)->codecpar->format}:in_sample_rate={((AVStream*)audioinputstream)->codecpar->sample_rate}:out_sample_fmt={((AVStream*)audiooutputstream)->codecpar->format}:out_sample_rate={((AVStream*)audiooutputstream)->codecpar->sample_rate}:out_chlayout=stereo";
		Console.WriteLine(resampleargs);
		/*   abuffersrcctx = (IntPtr)avfilter_graph_alloc_filter((AVFilterGraph*)afiltergraph, (AVFilter*)abuffersrcfilter, "ain");
           abuffersnkctx = (IntPtr)avfilter_graph_alloc_filter((AVFilterGraph*)afiltergraph, (AVFilter*)abuffersnkfilter, "aout");
   */


		//

		SwrContext* resamplectx = null;

		Console.WriteLine(av_get_sample_fmt_name((AVSampleFormat)audiodecoderctx->sample_fmt));
		Console.WriteLine(av_get_sample_fmt_name((AVSampleFormat)audioencoderctx->sample_fmt));
		succ = swr_alloc_set_opts2(&resamplectx,
		&audioencoderctx->ch_layout,
		audioencoderctx->sample_fmt,
		audioencoderctx->sample_rate,
		&audiodecoderctx->ch_layout,
		audiodecoderctx->sample_fmt,
		audiodecoderctx->sample_rate,
		0, null);
		checkprint(succ, "Error Setting Resampler options");

		succ = swr_init(resamplectx);
		checkprint(succ, "Error Initializing Resmapler");

		AVFilterContext* audiobuffersrc_context = (AVFilterContext*)abuffersrcctx;
		AVFilterContext* audiobuffersnk_context = (AVFilterContext*)abuffersnkctx;
		AVFilterContext* audioresample_context = (AVFilterContext*)resamplectx;

		succ = avfilter_graph_create_filter(&audiobuffersrc_context, (AVFilter*)abuffersrcfilter, "audio_in", audiobuffersrcargs, null, afiltergraph);
		checkprint(succ, "Error Creating Audio Buffer Source Filter");

		succ = avfilter_graph_create_filter(&audiobuffersnk_context, (AVFilter*)abuffersnkfilter, "audio_out", "", null, afiltergraph);
		checkprint(succ, "Error Creating Audio Buffer Sink Filter");

		succ = avfilter_graph_create_filter(&audioresample_context, (AVFilter*)resamplefilter, "audio_resample", resampleargs, null, afiltergraph);
		checkprint(succ, "Error Creating Audio Resample Filter");

		succ = avfilter_link(audiobuffersrc_context, 0, audioresample_context, 0);
		checkprint(succ, "Error Linkning Audio buffer source to resample filter");

		succ = avfilter_link(audioresample_context, 0, audiobuffersnk_context, 0);
		checkprint(succ, "Error Linkning Audio  resample filter to buffer sink");


		//Open Formats

		succ = avio_open(&outputcontext->pb, filename, AVIO_FLAG_WRITE);
		checkprint(succ, "Error Opening IO");

		//Write Format Header

		succ = avformat_write_header(outputcontext, null);
		checkprint(succ, "Error Writing Header to Output Format");


		AVFrame* ainput = av_frame_alloc();
		AVFrame* aoutput = av_frame_alloc();

		av_dump_format((AVFormatContext*)outputcontext, 0, filename, 1);

		//Stream Video
		foreach (var framcount in Enumerable.Range(0, videoencoderctx->framerate.num * 5))
		{
			Console.WriteLine("\nDoinf count");
			succ = av_read_frame((AVFormatContext*)videoinputfmt, (AVPacket*)videoinputpacket);
			checkprint(succ, "Error Reading Video Frame from Input Format");

			succ = av_read_frame((AVFormatContext*)audioinputfmt, (AVPacket*)audioinputpacket);
			checkprint(succ, "Error Reading Audio Frame from Input Format");
			//Audio Decoding
			do
			{
				succ = avcodec_send_packet(audiodecoderctx, (AVPacket*)audioinputpacket);
				checkprint(succ, "Error Sending Packet to Decoder");

				succ = avcodec_receive_frame(audiodecoderctx, ainput);
				checkprint(succ, "Error  Receiving Frame from Audio Decoder");

				/*Console.WriteLine(ainput->sample_rate);
				Console.WriteLine(ainput->nb_samples);
				Console.WriteLine(av_get_sample_fmt_name((AVSampleFormat)ainput->format));*/
				//Audio
				succ=av_buffersrc_add_frame(audiobuffersrc_context, ainput);
				//succ = swr_convert_frame(resamplectx, aoutput, ainput);
				checkprint(succ, "Error Resampling Audio input frame");

			}
			while (succ < 0);
			{
				succ = avcodec_send_packet(audiodecoderctx, (AVPacket*)audioinputpacket);
				checkprint(succ, "Error Sending Packet to Decoder");

				succ = avcodec_receive_frame(audiodecoderctx, ainput);
				checkprint(succ, "Error  Receiving Frame from Audio Decoder");



			}

			//Video Decoding
			do
			{
				succ = avcodec_send_packet(videodecoderctx, (AVPacket*)videoinputpacket);
				checkprint(succ, "Error Sending Packet to Audio Decoder");

				succ = avcodec_receive_frame(videodecoderctx, (AVFrame*)videoinputframe);
				checkprint(succ, "Error  Receiving Frame from Audio Decoder");


			}
			while (succ < 0);
			{
				succ = avcodec_send_packet(videodecoderctx, (AVPacket*)videoinputpacket);
				checkprint(succ, "Error Sending Packet to Video Decoder");

				succ = avcodec_receive_frame(videodecoderctx, (AVFrame*)videoinputframe);
				checkprint(succ, "Error  Receiving Frame from Video Decoder");

			}

			//Filtering
			//Video 
			succ = av_buffersrc_add_frame(videobuffersrc_context, (AVFrame*)videoinputframe);
			checkprint(succ, "Error feeding Video Buffer Source video input frames");

			succ = av_buffersink_get_frame(videobuffersnk_context, (AVFrame*)videooutputframe);
			checkprint(succ, "Error getting Video Frame from video buffer sink");






			/*  succ = av_buffersrc_add_frame(audiobuffersrc_context, (AVFrame*)audioinputframe);
              checkprint(succ, "Error feeding Audio Buffer Source audio input frames");
  
              succ = av_buffersink_get_frame(audiobuffersnk_context, (AVFrame*)audiooutputframe);
              checkprint(succ, "Error getting Audio Frame from Audio buffer sink");*/




		}
		//Close Formats and codecs
		//Clean Up
	}
	public string FrameToImage64(string fileurl = "C:\\Users\\USER\\Videos\\ScreenRecorderFiles\\20230630\\14-41-08.mp4")
	{
		
		int streamindex = 0;
		string image = "";

		byte* inputdata;

		//Base64;
		int succ = 0;
		AVFormatContext* inputfmtptr = (AVFormatContext*)(inputfmt.ToPointer());
		succ = avformat_open_input(&inputfmtptr, fileurl, null, null);
		checkprint(succ, "Error Opening input file");

		succ = avformat_find_stream_info(inputfmtptr, null);
		checkprint(succ, "Error Reading Format Context");

		foreach (int count in Enumerable.Range(0, (int)(inputfmtptr->nb_streams)))
		{
			if (inputfmtptr->streams[count]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
			{
				inputstream = inputfmtptr->streams[count];
				decoder = avcodec_find_decoder(inputstream->codecpar->codec_id);
				decoderctx = avcodec_alloc_context3(decoder);
				succ = avcodec_parameters_to_context(decoderctx, inputstream->codecpar);
				checkprint(succ, "Failed To Copy  Decoder Params From Stream");
				succ =avcodec_open2(decoderctx, decoder, null);
				checkprint(succ, "Error Opening Decoder");
				streamindex = count;
			}
		}
		av_dump_format(inputfmtptr, 0, fileurl, 0);
		for (int count = 0; count < 10; count++)
		{
			succ = av_read_frame(inputfmtptr, inputpacket);
			checkprint(succ, "Failed to Read Input Packet");
			if (inputpacket->stream_index == streamindex)
			{
				do
				{
					succ = avcodec_send_packet(decoderctx, inputpacket);
					checkprint(succ, "Error Sending Packet");
					succ = avcodec_receive_frame(decoderctx, inputframe);
					checkprint(succ, "Error Receiving  Frame");
				}
				while (succ < 0);


				// Convert the frame to a Bitmap
				var bitmap = new Bitmap(inputframe->width, inputframe->height, PixelFormat.Format24bppRgb);
				var bitmapData = bitmap.LockBits(new Rectangle(0, 0, inputframe->width, inputframe->height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
				ffmpeg.sws_scale(
				ffmpeg.sws_getCachedContext(null, inputframe->width, inputframe->height, decoderctx->pix_fmt,
				        inputframe->width, inputframe->height, AVPixelFormat.AV_PIX_FMT_BGR24,
				ffmpeg.SWS_BICUBIC, null, null, null),
				inputframe->data, inputframe->linesize, 0, inputframe->height, new[] { (byte*)bitmapData.Scan0.ToPointer() },
				    new[] { bitmapData.Stride });
				bitmap.UnlockBits(bitmapData);

				// Convert the Bitmap to a base64 encoded string
				using (var stream = new MemoryStream())
				{
					bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
					image = Convert.ToBase64String(stream.ToArray());
				}
				break;

			}
			

		}
		return image;

	}
	public void ReadFrame() 
	{
	}
	private void ProcessAudioAndGenerateWaveform(byte[] audioData, int sampleRate)
	{
		
	}

}
public struct  NameString 
{
	public string First { get; set; } = string.Empty;
	public string Last { get; set; } = string.Empty;
	public NameString() 
	{
	}
}
public unsafe struct Employee:IDisposable
{
	int* Id=null;
	public IntPtr Name =IntPtr.Zero;
	public string NameString { get => ((NameString*)Name.ToPointer())->First + "\t" + ((NameString*)Name.ToPointer())->Last; set => ((NameString*)Name.ToPointer())->First = value; }
	public float* Salary = null;
	public Employee() 
	{
		Id = (int*)av_malloc(sizeof(int));
                    Name = (IntPtr)av_malloc((ulong)sizeof(string));
                    Salary = (float*)av_malloc(sizeof(float));
          }
	public void SetNameReference(string * name,string lastname="Guan") 
	{
		NameString * namestring =(NameString*) av_malloc((ulong) sizeof(NameString));
		namestring->First = *name;
		namestring->Last = lastname; 
		
                    Name = (IntPtr)namestring;
		Console.WriteLine($"Employee name is {((NameString*)Name.ToPointer())->First} {((NameString*)Name.ToPointer())->Last}");
	}
	public void InitName(string name) 
	{
		string* nameptr = &name;
		SetNameReference(nameptr);
	}
	public NameString * Reference() 
	{
		return ((NameString*)Name.ToPointer());
	}

          public void Dispose()
          {
		av_free(Id);
		av_free(((NameString*)Name.ToPointer()));
		av_free(Salary);
          }
}
class Program
{
	public static unsafe void Main(string[] args)
	{
		/*Stream stream = new Stream();

		Console.WriteLine(stream.FrameToImage64());*/
		/*Employee ben = new Employee();
		ben.InitName("Ben");
		string name = ben.NameString;
		Console.WriteLine(name);
		ben.NameString = "Benson";
		Console.WriteLine(ben.NameString);
		Console.WriteLine(ben.Reference()->First);*/
		// Iterate over the input devices
	
                    AVDeviceInfoList* deviceList;
                    ffmpeg.avdevice_register_all();
		AVInputFormat* inputformat = av_find_input_format("dshow");
	
		avdevice_list_input_sources(inputformat, null, null, &deviceList);

                    // Print the device names
                    for (int i = 0; i < deviceList->nb_devices; i++)
                    {
                              var device = deviceList->devices[i];
                              string deviceName = Marshal.PtrToStringUTF8((IntPtr)device->device_name);
                              Console.WriteLine(deviceName);
                    }

                    // Free the device list
                    ffmpeg.avdevice_free_list_devices(&deviceList);

          }
}