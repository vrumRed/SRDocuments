#include <bits/stdc++.h>
#define n 1000

using namespace std;

int v[(2*n*n)+2][20][2];

int main(){
  int a = 0;
  memset(v, -1, sizeof(v));
  for(int i = 1; i <= n; i++){
    for(int j = 1; j <= n; j++){
      int k = 0;
      //a++;
      while(v[i*i+j*j][k][0] != -1){
	//a++;
	k++;
      }
      v[i*i+j*j][k][0] = i;
      v[i*i+j*j][k][1] = j;
    }
  }

  for(int i = 0; i <= 2*n*n; i++){
    for(int k = 0; v[i][k][0] != -1; k++){
      for(int j = 0; v[i][j][0] != -1; j++){
	printf("%d %d %d %d\n", v[i][k][0], v[i][k][1], v[i][j][0], v[i][j][1]);
	a++;
      }
    }
  }
  printf("%d\n", a);
}
