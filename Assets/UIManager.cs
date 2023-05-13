using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public TextMeshProUGUI currentScore;
    public TextMeshProUGUI totalScore;
    private int currentScoreInt = 0;
    private int totalScoreInt = 0;
    public TextMeshProUGUI currentBullets;
    private int currentBulletsInt = 0;
    public Image speedBar;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        ResetUI();
    }
    public void ResetUI()
    {
        currentScoreInt = 0;
        currentScore.text = currentScoreInt.ToString();
        currentBulletsInt = 0;
        currentBullets.text = currentBulletsInt.ToString();
    }

    public void AddPoint()
    {
        currentScoreInt ++;
        currentScore.text = currentScoreInt.ToString();
    }
    public void AddToTotalScore()
    {
        totalScoreInt++;
        totalScore.text = totalScoreInt.ToString();
    }

    public void AddBullet()
    {
        currentBulletsInt++;
        currentBullets.text = currentBulletsInt.ToString();
    }    
    public void SetSpeed(float currentSpeed, float maxSpeed, float minSpeed)
    {
      speedBar.fillAmount = (currentSpeed-minSpeed)/(maxSpeed-minSpeed);
    }

}
