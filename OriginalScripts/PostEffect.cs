using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Linq;

public class PostEffect : MonoBehaviour
{

    [SerializeField] private Shader _shader;
    [SerializeField] private AROcclusionManager occlusionManager;
    private Material _material;

    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private MusicController musicController;
    private double _fpsBuffer = 0.02d;

    float[] _waveData = new float[1024];
    float[] _spectrum = new float[1024];
    float _maxSpectrumValue = 0f;
    int _maxSpectrumIndex = 0;

    int _beatCount = 0;
    bool _doOnceBeatCount = true;
    bool _doOnceInit = true;

    float[] spectrumSum = new float[50]
    {   0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
        0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
        0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
        0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
        0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f
    };

    float[] _biginningWaveData = new float[441000];//10秒分の波形データ
    float[] _middleWaveData = new float[441000];//10秒分の波形データ
    float[] _endWaveData = new float[441000];//10秒分の波形データ
    float _aveVolume; //音量の平均
    float _noiseIntensityByVolume;

    void Awake()
    {
        _material = new Material(_shader);
    }


    private void Start()
    {
        audioSource.clip.GetData(_biginningWaveData, 0); //曲0秒〜10秒分の波形データ
        audioSource.clip.GetData(_middleWaveData, 1323000);//曲30秒〜40秒分の波形データ
        audioSource.clip.GetData(_endWaveData, 2646000);//曲60秒〜70秒分の波形データ
        _aveVolume = (_biginningWaveData.Select(x => x * x).Sum() + _middleWaveData.Select(x => x * x).Sum() + _endWaveData.Select(x => x * x).Sum()) / (_biginningWaveData.Length * 3);
    }

    //カメラ映像のレンダリング完了をトリガーに、ポストエフェクト実行
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (occlusionManager != null)
        {
            _material.SetTexture("_StencilTex", occlusionManager.humanStencilTexture);
        }
        Graphics.Blit(source, destination, _material);
    }


    private void FixedUpdate()
    {
        //音源から現在の音量取得
        //GetOutputData()で音源の波形データを配列個数分取得。（周波数成分に分けていない総合的な波形データ）
        AudioListener.GetOutputData(_waveData, 1);
        var volume = _waveData.Select(x => x * x).Sum() / _waveData.Length;
        _noiseIntensityByVolume = volume / _aveVolume / 4; //曲中における現在の相対的な音量の大きさ。４で割っているのはいい感じのエフェクトにするための微調整。


        //音源から現在のピッチ取得 
        //各周波成分で一番値が大きい＝振幅が大きい=音量が大きいものを基音（ピッチ）とする。
        //16拍子 = 4小節ごとにピッチの見直し
        if (_beatCount % 16 == 0)
        {
            if (_doOnceInit)
            {
                _maxSpectrumValue = 0;
                _maxSpectrumIndex = 0;
                _doOnceInit = false;
            }
            //サンプリングレートが44100Hzなので、実際の最大周波数は44100/2 Hz（ナイキスト周波数）
            //_spectrumの配列数が1024なので、44100/2/1024Hzごとに周波数成分を取得
            AudioListener.GetSpectrumData(_spectrum, 0, FFTWindow.Rectangular);
            for (int i = 0; i < _spectrum.Length; i++)
            {
                float val = _spectrum[i];
                if (val > _maxSpectrumValue)
                {
                    _maxSpectrumValue = val;
                    _maxSpectrumIndex = i;
                }
            }
        }
        else if(_beatCount % 16 == 8)
        {
            _doOnceInit = true;
        }
        //サンプリングレートが44100Hzなので、実際の最大周波数は44100/2 Hz（ナイキスト周波数）
        //_spectrumの配列数が1024なので、44100/2/1024Hzごとに周波数成分を取得
        //よって、一番音量が大きい周波数成分のピッチは、配列番号*44100/2/1024Hz
        var mainFreq = _maxSpectrumIndex * AudioSettings.outputSampleRate / 2 / _spectrum.Length;

        //BPMとピッチごとに色を変更
        if (musicController.bpm >= 140)
        {
            if (mainFreq >= 1046.502)
            {
                _material.SetColor("_Color", new Color(0f, 1f, 1f, 1f));
            }
            else if (mainFreq >= 739.989)
            {
                _material.SetColor("_Color", new Color(0f, 1f, 0.8f, 1f));
            }
            else if(mainFreq >= 523.251)
            {
                _material.SetColor("_Color", new Color(0f, 0.6f, 0.6f, 1f));
            }
            else if (mainFreq >= 369.994)
            {
                _material.SetColor("_Color", new Color(0f, 0.4f, 0.4f, 1f));
            }
            else if(mainFreq >= 261.626)
            {
                _material.SetColor("_Color", new Color(0f, 0.2f, 0.4f, 1f));
            }
            else
            {
                _material.SetColor("_Color", new Color(0f, 0.2f, 0.2f, 1f));
            }
        }
        else if (musicController.bpm >= 100)
        {
            if (mainFreq >= 1046.502)
            {
                _material.SetColor("_Color", new Color(1f, 1f, 0.8f, 1f));
            }
            else if (mainFreq >= 739.989)
            {
                _material.SetColor("_Color", new Color(1f, 0.8f, 0.4f, 1f));
            }
            else if (mainFreq >= 523.251)
            {
                _material.SetColor("_Color", new Color(1f, 0.6f, 0.2f, 1f));
            }
            else if (mainFreq >= 369.994)
            {
                _material.SetColor("_Color", new Color(1f, 0.6f, 0.6f, 1f));
            }
            else if (mainFreq >= 261.626)
            {
                _material.SetColor("_Color", new Color(1f, 0.4f, 0.6f, 1f));
            }
            else
            {
                _material.SetColor("_Color", new Color(1f, 0.4f, 1f, 1f));
            }
        }
        else
        {
            if (mainFreq >= 1046.502)
            {
                _material.SetColor("_Color", new Color(0.6f, 1f, 0.8f, 1f));
            }
            else if (mainFreq >= 739.989)
            {
                _material.SetColor("_Color", new Color(0.6f, 1f, 1f, 1f));
            }
            else if (mainFreq >= 523.251)
            {
                _material.SetColor("_Color", new Color(0.6f, 0.6f, 1f, 1f));
            }
            else if (mainFreq >= 369.994)
            {
                _material.SetColor("_Color", new Color(0.6f, 0.4f, 0.6f, 1f));
            }
            else if (mainFreq >= 261.626)
            {
                _material.SetColor("_Color", new Color(0.6f, 0.4f, 0.4f, 1f));
            }
            else
            {
                _material.SetColor("_Color", new Color(0.6f, 0.2f, 0.2f, 1f));
            }
        }

        /*
        //周波数成分を10段階に分割し、オーディオスペクトラムを基にテクスチャにノイズを適用
        for (var i = 0; i <= 103; i++)
        {
            if (i == 103)
            {
                spectrumSum[0] = 0f;   
            }
            else
            {
                spectrumSum[0] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum1", spectrumSum[0]/10 * sign);
            }
        }
        for (var i = 103; i <= 205; i++)
        {
            if (i == 205)
            {
                spectrumSum[1] = 0f;
            }
            else
            {
                spectrumSum[1] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum2", spectrumSum[1]/10 * sign);
            }
        }
        for (var i = 205; i <= 308; i++)
        {
            if (i == 308)
            {
                spectrumSum[2] = 0f;
            }
            else
            {
                spectrumSum[2] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum3", spectrumSum[2]/10 * sign);
            }
        }
        for (var i = 308; i <= 410; i++)
        {
            if (i == 410)
            {
                spectrumSum[3] = 0f;
            }
            else
            {
                spectrumSum[3] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum4", spectrumSum[3]/10 * sign);
            }
        }
        for (var i = 410; i <= 512; i++)
        {
            if (i == 512)
            {
                spectrumSum[4] = 0f;
            }
            else
            {
                spectrumSum[4] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum5", spectrumSum[4]/10 * sign);
            }
        }
        for (var i = 513; i <= 615; i++)
        {
            if (i == 615)
            {
                spectrumSum[5] = 0f;
            }
            else
            {
                spectrumSum[5] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum6", spectrumSum[5]/10 * sign);
            }
        }
        for (var i = 615; i <= 717; i++)
        {
            if (i == 717)
            {
                spectrumSum[6] = 0f;
            }
            else
            {
                spectrumSum[6] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum7", spectrumSum[6]/10 * sign);
            }
        }
        for (var i = 717; i <= 820; i++)
        {
            if (i == 820)
            {
                spectrumSum[7] = 0f;
            }
            else
            {
                spectrumSum[7] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum8", spectrumSum[7]/10 * sign);
            }
        }
        for (var i = 820; i <= 922; i++)
        {
            if (i == 922)
            {
                spectrumSum[8] = 0f;
            }
            else
            {
                spectrumSum[8] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum9", spectrumSum[8]/10 * sign);
            }
        }
        for (var i = 922; i <= 1024; i++)
        {
            if (i == 1024)
            {
                spectrumSum[9] = 0f;
            }
            else
            {
                spectrumSum[9] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum10", spectrumSum[9]/10 * sign);
            }
        }
        */

        //周波数成分を50段階に分割し、オーディオスペクトラムを基にテクスチャにノイズを適用
        //配列数1024/50==20.48
        //それぞれのfor内で初期化処理を実行するため、iの最大値を小数点切り上げ
        for (var i = 0; i <= 21; i++)
        {
            if (i == 21)
            {
                spectrumSum[0] = 0f;
            }
            else
            {
                spectrumSum[0] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum1", spectrumSum[0] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 21; i <= 41; i++)
        {
            if (i == 41)
            {
                spectrumSum[1] = 0f;
            }
            else
            {
                spectrumSum[1] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum2", spectrumSum[1] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 41; i <= 62; i++)
        {
            if (i == 62)
            {
                spectrumSum[2] = 0f;
            }
            else
            {
                spectrumSum[2] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum3", spectrumSum[2] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 62; i <= 82; i++)
        {
            if (i == 82)
            {
                spectrumSum[3] = 0f;
            }
            else
            {
                spectrumSum[3] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum4", spectrumSum[3] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 82; i <= 103; i++)
        {
            if (i == 103)
            {
                spectrumSum[4] = 0f;
            }
            else
            {
                spectrumSum[4] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum5", spectrumSum[4] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 103; i <= 123; i++)
        {
            if (i == 123)
            {
                spectrumSum[5] = 0f;
            }
            else
            {
                spectrumSum[5] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum6", spectrumSum[5] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 123; i <= 144; i++)
        {
            if (i == 144)
            {
                spectrumSum[6] = 0f;
            }
            else
            {
                spectrumSum[6] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum7", spectrumSum[6] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 144; i <= 164; i++)
        {
            if (i == 164)
            {
                spectrumSum[7] = 0f;
            }
            else
            {
                spectrumSum[7] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum8", spectrumSum[7] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 164; i <= 185; i++)
        {
            if (i == 185)
            {
                spectrumSum[8] = 0f;
            }
            else
            {
                spectrumSum[8] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum9", spectrumSum[8] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 185; i <= 205; i++)
        {
            if (i == 205)
            {
                spectrumSum[9] = 0f;
            }
            else
            {
                spectrumSum[9] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum10", spectrumSum[9] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 205; i <= 226; i++)
        {
            if (i == 226)
            {
                spectrumSum[10] = 0f;
            }
            else
            {
                spectrumSum[10] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum11", spectrumSum[10] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 226; i <= 246; i++)
        {
            if (i == 246)
            {
                spectrumSum[11] = 0f;
            }
            else
            {
                spectrumSum[11] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum12", spectrumSum[11] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 246; i <= 267; i++)
        {
            if (i == 267)
            {
                spectrumSum[12] = 0f;
            }
            else
            {
                spectrumSum[12] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum13", spectrumSum[12] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 267; i <= 287; i++)
        {
            if (i == 287)
            {
                spectrumSum[13] = 0f;
            }
            else
            {
                spectrumSum[13] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum14", spectrumSum[13] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 287; i <= 308; i++)
        {
            if (i == 308)
            {
                spectrumSum[14] = 0f;
            }
            else
            {
                spectrumSum[14] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum15", spectrumSum[14] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 308; i <= 328; i++)
        {
            if (i == 328)
            {
                spectrumSum[15] = 0f;
            }
            else
            {
                spectrumSum[15] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum16", spectrumSum[15] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 328; i <= 349; i++)
        {
            if (i == 349)
            {
                spectrumSum[16] = 0f;
            }
            else
            {
                spectrumSum[16] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum17", spectrumSum[16] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 349; i <= 369; i++)
        {
            if (i == 369)
            {
                spectrumSum[17] = 0f;
            }
            else
            {
                spectrumSum[17] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum18", spectrumSum[17] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 369; i <= 390; i++)
        {
            if (i == 390)
            {
                spectrumSum[18] = 0f;
            }
            else
            {
                spectrumSum[18] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum19", spectrumSum[18] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 390; i <= 410; i++)
        {
            if (i == 410)
            {
                spectrumSum[19] = 0f;
            }
            else
            {
                spectrumSum[19] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum20", spectrumSum[19] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 410; i <= 431; i++)
        {
            if (i == 431)
            {
                spectrumSum[20] = 0f;
            }
            else
            {
                spectrumSum[20] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum21", spectrumSum[20] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 431; i <= 451; i++)
        {
            if (i == 451)
            {
                spectrumSum[21] = 0f;
            }
            else
            {
                spectrumSum[21] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum22", spectrumSum[21] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 451; i <= 472; i++)
        {
            if (i == 472)
            {
                spectrumSum[22] = 0f;
            }
            else
            {
                spectrumSum[22] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum23", spectrumSum[22] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 472; i <= 492; i++)
        {
            if (i == 492)
            {
                spectrumSum[23] = 0f;
            }
            else
            {
                spectrumSum[23] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum24", spectrumSum[23] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 492; i <= 512; i++)
        {
            if (i == 512)
            {
                spectrumSum[24] = 0f;
            }
            else
            {
                spectrumSum[24] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum25", spectrumSum[24] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 512; i <= 533; i++)
        {
            if (i == 533)
            {
                spectrumSum[25] = 0f;
            }
            else
            {
                spectrumSum[25] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum26", spectrumSum[25] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 533; i <= 553; i++)
        {
            if (i == 553)
            {
                spectrumSum[26] = 0f;
            }
            else
            {
                spectrumSum[26] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum27", spectrumSum[26] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 553; i <= 574; i++)
        {
            if (i == 574)
            {
                spectrumSum[27] = 0f;
            }
            else
            {
                spectrumSum[27] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum28", spectrumSum[27] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 574; i <= 594; i++)
        {
            if (i == 594)
            {
                spectrumSum[28] = 0f;
            }
            else
            {
                spectrumSum[28] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum29", spectrumSum[28] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 594; i <= 615; i++)
        {
            if (i == 615)
            {
                spectrumSum[29] = 0f;
            }
            else
            {
                spectrumSum[29] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum30", spectrumSum[29] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 615; i <= 635; i++)
        {
            if (i == 635)
            {
                spectrumSum[30] = 0f;
            }
            else
            {
                spectrumSum[30] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum31", spectrumSum[30] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 635; i <= 656; i++)
        {
            if (i == 656)
            {
                spectrumSum[31] = 0f;
            }
            else
            {
                spectrumSum[31] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum32", spectrumSum[31] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 656; i <= 676; i++)
        {
            if (i == 676)
            {
                spectrumSum[32] = 0f;
            }
            else
            {
                spectrumSum[32] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum33", spectrumSum[32] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 676; i <= 697; i++)
        {
            if (i == 697)
            {
                spectrumSum[33] = 0f;
            }
            else
            {
                spectrumSum[33] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum34", spectrumSum[33] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 697; i <= 717; i++)
        {
            if (i == 717)
            {
                spectrumSum[34] = 0f;
            }
            else
            {
                spectrumSum[34] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum35", spectrumSum[34] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 717; i <= 738; i++)
        {
            if (i == 738)
            {
                spectrumSum[35] = 0f;
            }
            else
            {
                spectrumSum[35] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum36", spectrumSum[35] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 738; i <= 758; i++)
        {
            if (i == 758)
            {
                spectrumSum[36] = 0f;
            }
            else
            {
                spectrumSum[36] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum37", spectrumSum[36] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 758; i <= 779; i++)
        {
            if (i == 779)
            {
                spectrumSum[37] = 0f;
            }
            else
            {
                spectrumSum[37] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum38", spectrumSum[37] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 779; i <= 799; i++)
        {
            if (i == 799)
            {
                spectrumSum[38] = 0f;
            }
            else
            {
                spectrumSum[38] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum39", spectrumSum[38] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 799; i <= 820; i++)
        {
            if (i == 820)
            {
                spectrumSum[39] = 0f;
            }
            else
            {
                spectrumSum[39] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum40", spectrumSum[39] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 820; i <= 840; i++)
        {
            if (i == 840)
            {
                spectrumSum[40] = 0f;
            }
            else
            {
                spectrumSum[40] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum41", spectrumSum[40]* sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 840; i <= 861; i++)
        {
            if (i == 861)
            {
                spectrumSum[41] = 0f;
            }
            else
            {
                spectrumSum[41] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum42", spectrumSum[41] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 861; i <= 881; i++)
        {
            if (i == 881)
            {
                spectrumSum[42] = 0f;
            }
            else
            {
                spectrumSum[42] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum43", spectrumSum[42] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 881; i <= 902; i++)
        {
            if (i == 902)
            {
                spectrumSum[43] = 0f;
            }
            else
            {
                spectrumSum[43] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum44", spectrumSum[43] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 902; i <= 922; i++)
        {
            if (i == 922)
            {
                spectrumSum[44] = 0f;
            }
            else
            {
                spectrumSum[44] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum45", spectrumSum[44] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 922; i <= 943; i++)
        {
            if (i == 943)
            {
                spectrumSum[45] = 0f;
            }
            else
            {
                spectrumSum[45] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum46", spectrumSum[45] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 943; i <= 963; i++)
        {
            if (i == 963)
            {
                spectrumSum[46] = 0f;
            }
            else
            {
                spectrumSum[46] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum47", spectrumSum[46] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 963; i <= 984; i++)
        {
            if (i == 984)
            {
                spectrumSum[47] = 0f;
            }
            else
            {
                spectrumSum[47] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum48", spectrumSum[47] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 984; i <= 1004; i++)
        {
            if (i == 1004)
            {
                spectrumSum[48] = 0f;
            }
            else
            {
                spectrumSum[48] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum49", spectrumSum[48] * sign * _noiseIntensityByVolume);
            }
        }
        for (var i = 1004; i <= 1024; i++)
        {
            if (i == 1024)
            {
                spectrumSum[49] = 0f;
            }
            else
            {
                spectrumSum[49] += _spectrum[i];
                int sign;
                if (Random.value <= 0.5f)
                {
                    sign = -1;
                }
                else
                {
                    sign = 1;
                }
                _material.SetFloat("_spectrum50", spectrumSum[49] * sign * _noiseIntensityByVolume);
            }
        }



        if (audioSource.isPlaying)
        {
            //拍がくる時にalpha値を１にする。拍から次の拍までに徐々にalpha値を0→1にする。
            //拍と拍の間のどれくらいにきているかは、各拍が何秒ごとにきて、曲の経過秒とその倍数秒を比較（割った余り）すればOK
            //例えば、120bpmは120拍60秒なので、1拍0.5秒
            double currentRemainderBeatIntervalD = AudioSettings.dspTime % (60d / musicController.bpm);
            float currentRemainderBeatIntervalF = (float)currentRemainderBeatIntervalD;
            //マシンの現実的な処理スピードを考慮して、_fpsBuffer(0.02秒)を設ける
            if (currentRemainderBeatIntervalD <= _fpsBuffer)
            {
                _material.SetFloat("_alpha", 1);
                if (_doOnceBeatCount)
                {
                    //拍数を数える
                    _beatCount += 1;
                    _doOnceBeatCount = false;
                }
            }
            else
            {
                _material.SetFloat("_alpha", (1 - (currentRemainderBeatIntervalF / (60f / musicController.bpm))));
                _doOnceBeatCount = true;
            }
        }
        else
        {
            _material.SetFloat("_alpha", 0);
        }
    }
}