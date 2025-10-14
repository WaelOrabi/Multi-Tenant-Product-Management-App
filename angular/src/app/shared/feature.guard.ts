import { inject } from '@angular/core';
import { CanMatchFn, Route, UrlSegment, Router } from '@angular/router';
import { ConfigStateService } from '@abp/ng.core';

export const featureCanMatch: CanMatchFn = (route: Route, _segments: UrlSegment[]) => {
  const config = inject(ConfigStateService);
  const router = inject(Router);
  const required = (route.data?.['requiredFeatures'] as string[]) ?? [];
  
  const enabled = required.every(f => {
    const featureValue = config.getFeature(f);
    return featureValue === 'true';
  });
  
  if (!enabled) {
    router.navigateByUrl('/');
    return false;
  }
  
  return true;
};