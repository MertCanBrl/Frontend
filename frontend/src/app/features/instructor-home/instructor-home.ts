import { Component, OnInit, OnDestroy, ViewChild, ElementRef, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { InstructorService } from '../../core/services/instructor.service';
import { InstructorDashboardDto, CourseSummaryReportDto } from '../../core/models/course.models';

Chart.register(...registerables);

@Component({
  selector: 'app-instructor-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './instructor-home.html',
  styleUrl: './instructor-home.css'
})
export class InstructorHome implements OnInit, OnDestroy {
  @ViewChild('poChart') poChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('trendChart') trendChartRef?: ElementRef<HTMLCanvasElement>;

  private instructorService = inject(InstructorService);
  private cdr = inject(ChangeDetectorRef);

  dashboard: InstructorDashboardDto | null = null;
  loading = true;
  trendType: 'GÜZ' | 'BAHAR' = 'GÜZ';
  selectedLoSemester = '';
  availableSemesters: string[] = [];

  private poChartInstance: Chart | null = null;
  private trendChartInstance: Chart | null = null;

  readonly circumference = 2 * Math.PI * 40;

  ngOnInit() {
    this.instructorService.getDashboard().subscribe({
      next: (data) => {
        this.dashboard = data;
        this.availableSemesters = [...new Set(data.averageTrend.map(t => t.semester))];
        if (this.availableSemesters.length > 0) {
          this.selectedLoSemester = this.availableSemesters[0];
        }
        this.loading = false;
        // Force DOM update then render charts
        this.cdr.detectChanges();
        this.renderPoChart();
        this.renderTrendChart();
      },
      error: () => { this.loading = false; }
    });
  }

  ngOnDestroy() {
    this.poChartInstance?.destroy();
    this.trendChartInstance?.destroy();
  }

  getOffset(course: CourseSummaryReportDto): number {
    const pct = course.classAverage ?? 0;
    return this.circumference - (pct / 100) * this.circumference;
  }

  getSuccessLabel(course: CourseSummaryReportDto): string {
    return course.classAverage !== null ? `${Math.round(course.classAverage!)}%` : '–';
  }

  switchTrend(type: 'GÜZ' | 'BAHAR') {
    this.trendType = type;
    this.renderTrendChart();
  }

  private renderPoChart() {
    if (!this.dashboard || !this.poChartRef) return;
    this.poChartInstance?.destroy();

    const data = this.dashboard.poContribution;
    const colors = ['#cfe2ff', '#7eb8f7', '#3c82d4', '#1a5aaa', '#0a3a7a'];

    this.poChartInstance = new Chart(this.poChartRef.nativeElement, {
      type: 'bar',
      data: {
        labels: data.map(p => p.poCode),
        datasets: [0, 1, 2, 3, 4].map(i => ({
          label: `Ağırlık ${i + 1}`,
          data: data.map(p => p.countsByWeight[i] ?? 0),
          backgroundColor: colors[i],
          stack: 'stack'
        }))
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'right', labels: { boxWidth: 12, font: { size: 11 } } },
          tooltip: {
            callbacks: {
              title: (items) => {
                const idx = items[0].dataIndex;
                return `${data[idx].poCode}: ${data[idx].poDescription}`;
              },
              label: (item) => ` ${item.dataset.label}: ${item.raw} ders`
            }
          }
        },
        scales: {
          x: { stacked: true, ticks: { font: { size: 10 } } },
          y: { stacked: true, ticks: { stepSize: 1 }, beginAtZero: true }
        }
      }
    });
  }

  private renderTrendChart() {
    if (!this.dashboard || !this.trendChartRef) return;
    this.trendChartInstance?.destroy();

    const keyword = this.trendType === 'GÜZ' ? 'Güz' : 'Bahar';
    const filtered = this.dashboard.averageTrend.filter(t =>
      t.semester.toLowerCase().includes(keyword.toLowerCase())
    );

    const semesters = [...new Set(filtered.map(t => t.semester))].sort();
    const codes = [...new Set(filtered.map(t => t.courseCode))];
    const palette = ['#16213e', '#3c82d4', '#e94560', '#533483', '#2ecc71', '#e67e22'];

    this.trendChartInstance = new Chart(this.trendChartRef.nativeElement, {
      type: 'line',
      data: {
        labels: semesters,
        datasets: codes.map((code, i) => {
          const courseRows = filtered.filter(t => t.courseCode === code);
          return {
            label: code,
            data: semesters.map(sem => courseRows.find(t => t.semester === sem)?.average ?? null),
            borderColor: palette[i % palette.length],
            backgroundColor: palette[i % palette.length],
            tension: 0,
            pointRadius: 5,
            pointHoverRadius: 7,
            spanGaps: false
          };
        })
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'right', labels: { boxWidth: 12, font: { size: 11 } } },
          tooltip: {
            callbacks: {
              label: (item) => ` ${item.dataset.label} – Ortalama: ${item.raw}`
            }
          }
        },
        scales: {
          y: { min: 0, max: 100, ticks: { stepSize: 10 } }
        }
      }
    });
  }
}
