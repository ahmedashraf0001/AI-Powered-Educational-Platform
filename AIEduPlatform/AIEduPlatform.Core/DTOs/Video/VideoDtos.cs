using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Video
{

    public class VideoAnalysisRequest
    {
        public float frame_interval_seconds { get; set; } = 5.0f;
        public int max_frames { get; set; } = 200;
        public bool transcribe { get; set; } = true;
        public bool analyze_visuals { get; set; } = true;
        public string language { get; set; } = "en";
        public bool include_timestamps { get; set; } = true;
        public bool summary_format { get; set; } = false;
    }

    public class VideoAnalysisResponse
    {
        public List<Segment> segments { get; set; }
        public string full_transcript { get; set; }
        public List<Frame_Analyses> frame_analyses { get; set; }
        public string llm_context { get; set; }
        public float video_duration_seconds { get; set; }
        public float processing_time_ms { get; set; }
        public Video_Dimensions video_dimensions { get; set; }
        public float fps { get; set; }
        public bool has_audio { get; set; }
        public int frames_analyzed { get; set; }
    }

    public class Video_Dimensions
    {
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Segment
    {
        public float start_time { get; set; }
        public float end_time { get; set; }
        public string visual_description { get; set; }
        public string transcript { get; set; }
    }

    public class Frame_Analyses
    {
        public float timestamp_seconds { get; set; }
        public string description { get; set; }
        public int frame_number { get; set; }
    }
}
