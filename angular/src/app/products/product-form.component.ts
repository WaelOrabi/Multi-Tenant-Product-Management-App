import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, effect, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ProductService } from 'src/app/proxy/products';
import { CreateUpdateProductDto, ProductDto, ProductVariantDto } from 'src/app/proxy/products/dtos';
import { ToasterService } from '@abp/ng.theme.shared';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.css']
})
export class ProductFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private service = inject(ProductService);
  private toaster = inject(ToasterService);

  form: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    basePrice: [null],
    category: [''],
    status: [0],
    hasVariants: [false],
    variants: this.fb.array([] as FormGroup[]),
  });

  get variants() { return this.form.get('variants') as FormArray<FormGroup>; }

  isEdit = false;
  id: string | null = null;
  saving = false;

  ngOnInit(): void {
    this.isEdit = (this.route.snapshot.data['mode'] || '') === 'edit';
    this.id = this.route.snapshot.paramMap.get('id');

    if (this.isEdit && this.id) {
      this.load();
    }

    this.form.get('hasVariants')!.valueChanges.subscribe(v => {
      if (v) {
        this.form.get('basePrice')!.setValue(null);
      }
    });
  }

  load(){
    if (!this.id) return;
    this.service.get(this.id).subscribe({
      next: (p) => {
        this.form.patchValue({
          name: p.name,
          description: p.description,
          basePrice: p.basePrice ?? null,
          category: p.category,
          status: p.status,
          hasVariants: p.hasVariants,
        });
        this.variants.clear();
        if (p.hasVariants && p.variants) {
          for (const v of p.variants) this.variants.push(this.createVariantGroup(v));
        }
      },
      error: err => { this.toaster.error('Failed to load product'); console.error(err); }
    });
  }

  createVariantGroup(v?: ProductVariantDto){
    return this.fb.group({
      sku: [v?.sku || '', Validators.required],
      price: [v?.price ?? 0, [Validators.required, Validators.min(0)]],
      stockQuantity: [v?.stockQuantity ?? 0, [Validators.required, Validators.min(0)]],
      attributesJson: [v?.attributesJson || ''],
    });
  }

  addVariant(){ this.variants.push(this.createVariantGroup()); }
  removeVariant(i: number){ this.variants.removeAt(i); }

  save(){
    if (this.form.invalid) return;
    this.saving = true;
    const value = this.form.value;
    const input: CreateUpdateProductDto = {
      name: value.name,
      description: value.description || undefined,
      basePrice: value.hasVariants ? null : (value.basePrice ?? null),
      category: value.category || undefined,
      status: value.status ?? 0,
      hasVariants: value.hasVariants,
      variants: value.hasVariants ? (value.variants as any) : undefined,
    };
    const obs = this.isEdit && this.id ? this.service.update(this.id, input) : this.service.create(input);
    obs.subscribe({
      next: () => { this.toaster.success('Saved'); this.router.navigate(['/products']); },
      error: err => { this.saving = false; this.toaster.error('Save failed'); console.error(err); }
    });
  }
}
