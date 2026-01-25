import whisper
import json
from pathlib import Path
import sys

def transcribe_audio(audio_file, model_name="large-v3-turbo"):
    try:
        # Load model with GPU support
        model = whisper.load_model(model_name, device="cuda")
        
        # Ensure audio_file is a string
        audio_path = str(audio_file) if isinstance(audio_file, Path) else audio_file
        
        # Transcribe with word-level timestamps
        result = model.transcribe(audio_path, word_timestamps=True)
        
        # Structure data for JSON
        json_data = {
            "words": [
                {
                    "segentID": idx,
                    "word": word_info["word"].lstrip().replace('"', '\\"').replace('\n', '\\n'),
                    "start": word_info["start"],
                    "end": word_info["end"]
                }
                for idx, segment in enumerate(result["segments"])
                for word_info in segment["words"]           
            ]
        }
        
        # Print JSON to console (pretty-printed)
        print(f"\nTranscription for {audio_file}:")
        print(json.dumps(json_data, indent=2, ensure_ascii=False))
        
        # Save JSON to file
        output_file = f"{audio_file.stem}-transcription.json"
        with open(output_file, "w", encoding="utf-8") as f:
            json.dump(json_data, f, indent=2, ensure_ascii=False)
        
        return result
    
    except Exception as e:
        print(f"Error processing {audio_file}: {str(e)}")
        return None

if __name__ == "__main__":
    # Get current directory
    current_dir = Path.cwd()
    
    # Find all .mp3 files in the current directory
    mp3_files = list(current_dir.glob("*.mp3"))
    
    if not mp3_files:
        print("No .mp3 files found in the current directory")
        sys.exit(1)
    
    # Process each .mp3 file
    for mp3_file in filter(lambda f: "01-Genesis-24" > f.name, mp3_files):
        print(f"Processing: {mp3_file}")
        transcribe_audio(mp3_file, model_name="large-v3-turbo")