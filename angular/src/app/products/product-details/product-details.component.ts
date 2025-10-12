import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ProductService } from 'src/app/proxy/products';
import { ProductDto, ProductVariantOptionDto } from 'src/app/proxy/products/dtos';
import { ToasterService } from '@abp/ng.theme.shared';
import { PermissionService, LocalizationPipe, LocalizationService } from '@abp/ng.core';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [CommonModule, RouterModule, LocalizationPipe],
  templateUrl: './product-details.component.html',
  styleUrls: ['./product-details.component.css']
})
export class ProductDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private service = inject(ProductService);
  private toaster = inject(ToasterService);
  private permission = inject(PermissionService);
  private l = inject(LocalizationService);

  product: ProductDto | null = null;
  loading = false;
  id: string | null = null;
  canEdit = false;
  canDelete = false;
  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id');
    this.permission.getGrantedPolicy$('MultiTenantProductManagementApp.Products.Edit').subscribe(g => this.canEdit = !!g);
    this.permission.getGrantedPolicy$('MultiTenantProductManagementApp.Products.Delete').subscribe(g => this.canDelete = !!g);
    if (this.id) {
      this.loadProduct();
    }
  }

  private loadProduct(): void {
    if (!this.id) return;
    
    this.loading = true;
    this.service.get(this.id).subscribe({
      next: (product) => {
        this.product = product;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.toaster.error(this.l.instant('::Product.Details.LoadFailed'));
        console.error(err);
      }
    });
  }

  private formatOptions(options?: ProductVariantOptionDto[] | null): string {
    const list = options ?? [];
    return list.map(o => `${o.name}: ${o.value}`).join(', ');
  }

  private confirmDelete(): void {
    if (!this.product) return;
    const msg = `${this.l.instant('::Product.Details.ConfirmDelete')} "${this.product.name}"?`;
    if (!confirm(msg)) return;
    
    this.loading = true;
    this.service.delete(this.product.id).subscribe({
      next: () => {
        this.toaster.success(this.l.instant('::Product.Details.Deleted'));
        this.router.navigate(['/products']);
      },
      error: (err) => {
        this.loading = false;
        this.toaster.error(this.l.instant('::Product.Details.DeleteFailed'));
        console.error(err);
      }
    });
  }
}
