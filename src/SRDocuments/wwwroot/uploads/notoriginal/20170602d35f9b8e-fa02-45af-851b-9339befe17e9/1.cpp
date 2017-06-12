#include <bits/stdc++.h>
#define n 5

using namespace std;

int main(){
  int a = 0;
  for(int i = 1; i <= n; i++){
    for(int j = 1; j <= n; j++){
      for(int k = 1; k <= n; k++){
	for(int l = 1; l <= n; l++){
	  if(i*i+j*j == k*k+l*l){
	    a++;
	  }
	}
      }
    }
  }
  printf("%d\n", a);
}
